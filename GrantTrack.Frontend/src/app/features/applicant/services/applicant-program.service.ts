import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GetProgram } from '../../../core/models/program.models';

/**
 * Read-only view of programs for the applicant role. Wraps the same
 * /program/GetPrograms endpoint Admin uses but filters out inactive
 * programs (you can't apply to those — see Messages.ProgramNotActive).
 */
@Injectable({ providedIn: 'root' })
export class ApplicantProgramService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/program`;

  /** Returns programs whose Status flag is true. */
  getActive(): Observable<GetProgram[]> {
    return this.http.get<GetProgram[]>(`${this.base}/GetPrograms`).pipe(
      map(list => list.filter(p => p.status === true)),
      catchError(this.toError),
    );
  }

  /** All programs (used by the home/landing page if we want to show counts). */
  getAll(): Observable<GetProgram[]> {
    return this.http.get<GetProgram[]>(`${this.base}/GetPrograms`)
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
