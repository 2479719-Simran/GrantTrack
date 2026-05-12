import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CreateDisbursementRequest,
  CreatePaymentRequest,
  DisbursementResponse,
  PaymentResponse,
  UpdateDisbursementRequest,
} from '../../../core/models/disbursement.models';

/**
 * Write-side API for disbursements + payments. Wraps the three endpoints
 * on the existing DisbursementController (FinanceOfficer-only):
 *   POST   /api/v1/disbursement              – create tranche
 *   PATCH  /api/v1/disbursement/{id}         – update tranche (amount/dates/status)
 *   POST   /api/v1/disbursement/payments     – record a payment
 *
 * The BE wraps successful responses in `{ message, data }`. We unwrap the
 * `data` here so callers get a flat DTO; the message still arrives in the
 * tap if a caller needs it (we ignore it for now and rely on toasts).
 */
@Injectable({ providedIn: 'root' })
export class DisbursementService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/disbursement`;

  /** POST /api/v1/disbursement. */
  create(req: CreateDisbursementRequest): Observable<DisbursementResponse> {
    return this.http
      .post<{ message: string; data: DisbursementResponse }>(this.base, req)
      .pipe(map(envelope => envelope.data), catchError(this.toError));
  }

  /** PATCH /api/v1/disbursement/{id}. */
  update(id: number, req: UpdateDisbursementRequest): Observable<DisbursementResponse> {
    return this.http
      .patch<{ message: string; data: DisbursementResponse }>(`${this.base}/${id}`, req)
      .pipe(map(envelope => envelope.data), catchError(this.toError));
  }

  /** POST /api/v1/disbursement/payments. */
  recordPayment(req: CreatePaymentRequest): Observable<PaymentResponse> {
    return this.http
      .post<{ message: string; data: PaymentResponse }>(`${this.base}/payments`, req)
      .pipe(map(envelope => envelope.data), catchError(this.toError));
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
