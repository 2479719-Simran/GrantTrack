import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  ComplianceApplicationDetail,
  ComplianceApplicationListRow,
} from '../../../core/models/compliance.models';

/**
 * Read-side API for the Compliance Officer's hub:
 *   GET /api/v1/compliance/applications        – queue of approved apps
 *   GET /api/v1/compliance/applications/{id}   – full detail with checks
 *
 * The schedule/complete writes live on a separate ComplianceCheckService
 * since they hit the existing /api/v1/compliancecheck endpoints.
 */
@Injectable({ providedIn: 'root' })
export class ComplianceService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/compliance`;

  list(): Observable<ComplianceApplicationListRow[]> {
    return this.http
      .get<ComplianceApplicationListRow[]>(`${this.base}/applications`)
      .pipe(catchError(this.toError));
  }

  getDetail(applicationId: number): Observable<ComplianceApplicationDetail> {
    return this.http
      .get<ComplianceApplicationDetail>(`${this.base}/applications/${applicationId}`)
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
