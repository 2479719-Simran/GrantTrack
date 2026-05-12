import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  FinanceApplicationDetail,
  FinanceApplicationListRow,
} from '../../../core/models/finance.models';

/**
 * Read-side API for the Finance Officer's hub:
 *   GET /api/v1/finance/applications        – approved apps + disbursement rollup
 *   GET /api/v1/finance/applications/{id}   – full detail with tranches + payments
 *
 * The write-side endpoints (create disbursement, update tranche, record
 * payment) live on the existing /api/v1/disbursement controller — see
 * DisbursementService for that.
 */
@Injectable({ providedIn: 'root' })
export class FinanceService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/finance`;

  list(): Observable<FinanceApplicationListRow[]> {
    return this.http
      .get<FinanceApplicationListRow[]>(`${this.base}/applications`)
      .pipe(catchError(this.toError));
  }

  getDetail(applicationId: number): Observable<FinanceApplicationDetail> {
    return this.http
      .get<FinanceApplicationDetail>(`${this.base}/applications/${applicationId}`)
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
