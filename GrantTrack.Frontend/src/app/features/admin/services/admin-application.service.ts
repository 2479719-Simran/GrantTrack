import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  AdminApplicationDetail,
  AdminApplicationListRow,
} from '../../../core/models/admin-application.models';

/**
 * Admin's "all applications" hub. Two endpoints:
 *   GET  /api/v1/admin/applications        – list every application
 *   GET  /api/v1/admin/applications/{id}   – full detail for one
 */
@Injectable({ providedIn: 'root' })
export class AdminApplicationService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/admin/applications`;

  list(): Observable<AdminApplicationListRow[]> {
    return this.http
      .get<AdminApplicationListRow[]>(this.base)
      .pipe(catchError(this.toError));
  }

  getDetail(applicationId: number): Observable<AdminApplicationDetail> {
    return this.http
      .get<AdminApplicationDetail>(`${this.base}/${applicationId}`)
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
