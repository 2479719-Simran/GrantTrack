import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { FinanceDisbursementInfo } from '../models/finance.models';

/**
 * Read-only disbursement listing for a single application — shared across
 * roles. Backed by GET /api/v1/applications/{id}/disbursements which
 * returns the same FinanceDisbursementInfo shape the Finance Officer's
 * detail page consumes.
 *
 * The BE owner-checks Applicant callers; other privileged roles (Admin,
 * Approver, FinanceOfficer, ComplianceOfficer) get blanket read.
 */
@Injectable({ providedIn: 'root' })
export class ApplicationDisbursementsService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiUrl;

  list(applicationId: number): Observable<FinanceDisbursementInfo[]> {
    return this.http
      .get<FinanceDisbursementInfo[]>(`${this.base}/applications/${applicationId}/disbursements`)
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
