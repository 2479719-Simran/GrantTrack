import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CompleteCheckRequest,
  ScheduleCheckRequest,
} from '../../../core/models/compliance.models';

/**
 * Write-side API for compliance checks. Wraps the two endpoints on the
 * existing ComplianceCheckController (ComplianceOfficer-only):
 *   POST   /api/v1/compliancecheck         – schedule a new check
 *   PATCH  /api/v1/compliancecheck/{id}    – complete (verify or flag) a check
 *
 * Returns are the BE entity itself; we type as `unknown` since the FE
 * re-fetches the parent detail page after each mutation rather than
 * reconciling the entity in place.
 */
@Injectable({ providedIn: 'root' })
export class ComplianceCheckService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/compliancecheck`;

  /** POST /api/v1/compliancecheck */
  schedule(req: ScheduleCheckRequest): Observable<unknown> {
    return this.http
      .post<unknown>(this.base, req)
      .pipe(catchError(this.toError));
  }

  /** PATCH /api/v1/compliancecheck/{id} */
  complete(checkId: number, req: CompleteCheckRequest): Observable<unknown> {
    return this.http
      .patch<unknown>(`${this.base}/${checkId}`, req)
      .pipe(catchError(this.toError));
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
