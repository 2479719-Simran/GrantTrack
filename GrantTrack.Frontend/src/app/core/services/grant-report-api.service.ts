import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, of, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GrantReport, GrantReportRequest } from '../models/grant-report.models';

/**
 * Shared API client for grant reports. Lives in core because both the
 * Applicant detail page (write + read) and downstream role pages
 * (Compliance / Admin / Approver / Finance — read only) consume it.
 *
 * The BE wraps create + update behind a single POST upsert, so this
 * service exposes just two methods: `upsert` and `getByApplication`.
 */
@Injectable({ providedIn: 'root' })
export class GrantReportApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/grantreport`;

  /** POST /api/v1/grantreport — create or update for the calling applicant. */
  upsert(req: GrantReportRequest): Observable<GrantReport> {
    return this.http
      .post<GrantReport>(this.base, req)
      .pipe(catchError(this.toError));
  }

  /**
   * GET /api/v1/grantreport/by-application/{id}.
   * BE returns 204 NoContent (empty body) when there's no report yet, so
   * we use observe:'response' and translate that into a clean null result
   * for the caller.
   */
  getByApplication(applicationId: number): Observable<GrantReport | null> {
    return this.http
      .get<GrantReport>(`${this.base}/by-application/${applicationId}`,
        { observe: 'response' as const })
      .pipe(
        map(res => res.status === 204 ? null : (res.body ?? null)),
        catchError((err: HttpErrorResponse) => {
          // 404 also means "no report" in the failure case — surface as null
          // so the FE can render the empty state instead of an error.
          if (err.status === 404) return of(null);
          return this.toError(err);
        }),
      );
  }

  private toError(err: HttpErrorResponse) {
    const msg = (typeof err.error === 'string' && err.error)
      || err.error?.error
      || err.error?.message
      || (err.error?.errors && Object.values<string[]>(err.error.errors)[0]?.[0])
      || `Request failed (HTTP ${err.status}).`;
    return throwError(() => new Error(msg));
  }
}
