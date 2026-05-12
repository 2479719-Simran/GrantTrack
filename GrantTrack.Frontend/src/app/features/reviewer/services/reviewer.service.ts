import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  RecommendationRequest,
  ReviewFilterRequest,
  ReviewFilterResponse,
} from '../../../core/models/review.models';
import { ReviewDecision } from '../../../core/models/enums';

/**
 * Reviewer-only API surface. Two endpoints:
 *   • GET  /api/v1/reviewfilter/reviews   – paginated list of MY reviews
 *   • POST /api/v1/recommendation/recommendation – submit my recommendation
 *
 * The list endpoint accepts decision as a query param to filter by status.
 * Score on the BE must satisfy /^([1-9]|10)$/ — we mirror that on the FE
 * so we don't waste a round-trip on bad input.
 */
@Injectable({ providedIn: 'root' })
export class ReviewerService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiUrl;

  /**
   * GET /api/v1/reviewfilter/reviews
   * Backend signature: ReviewFilterRequestDto bound from query string.
   * We pass the reviewer id explicitly because the backend doesn't pull it
   * from the JWT claims.
   */
  listAssigned(req: ReviewFilterRequest): Observable<ReviewFilterResponse[]> {
    let params = new HttpParams()
      .set('reviewerId', String(req.reviewerId))
      .set('pageNumber', String(req.pageNumber ?? 1))
      .set('pageSize', String(req.pageSize ?? 10));
    if (req.decision) {
      params = params.set('decision', req.decision);
    }
    return this.http
      .get<ReviewFilterResponse[]>(`${this.base}/reviewfilter/reviews`, { params })
      .pipe(catchError(this.toError));
  }

  /**
   * POST /api/v1/recommendation/recommendation
   * Returns { message: string } on success. The BE returns 200 even if the
   * recommendation was rejected by validation, with the error in the body —
   * caller should still inspect the toast/result.
   */
  submitRecommendation(req: RecommendationRequest): Observable<{ message: string }> {
    return this.http
      .post<{ message: string }>(`${this.base}/recommendation/recommendation`, req)
      .pipe(catchError(this.toError));
  }

  /** Decision options the reviewer can pick — Pending is excluded because
   * submitting "Pending" defeats the purpose of recording a recommendation. */
  static readonly SUBMITTABLE_DECISIONS: ReviewDecision[] = ['Approved', 'Rejected'];

  private toError(err: HttpErrorResponse) {
    const msg = (typeof err.error === 'string' && err.error)
      || err.error?.error
      || err.error?.message
      || (err.error?.errors && Object.values<string[]>(err.error.errors)[0]?.[0])
      || `Request failed (HTTP ${err.status}).`;
    return throwError(() => new Error(msg));
  }
}
