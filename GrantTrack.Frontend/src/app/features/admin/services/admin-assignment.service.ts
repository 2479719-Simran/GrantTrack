import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { BulkAssignmentRequest } from '../../../core/models/review.models';

/**
 * Admin reviewer-assignment client. The backend currently exposes only
 * the bulk-assign endpoint; there's no GET listing applications-needing-
 * assignment, so the page collects IDs from the admin manually.
 */
@Injectable({ providedIn: 'root' })
export class AdminAssignmentService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/reviews/assignments`;

  /** POST /api/v1/reviews/assignments */
  bulkAssign(req: BulkAssignmentRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(this.url, req)
      .pipe(catchError(this.toError));
  }

  private toError(err: HttpErrorResponse) {
    const msg = (typeof err.error === 'string' && err.error)
      || err.error?.error
      || err.error?.message
      || `Request failed (HTTP ${err.status}).`;
    return throwError(() => new Error(msg));
  }
}
