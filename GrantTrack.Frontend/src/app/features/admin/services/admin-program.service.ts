import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CreateProgramRequest,
  CreateProgramResponse,
  DeleteProgramResponse,
  FilterProgramsRequest,
  GetProgram,
  UpdateProgramRequest,
  UpdateProgramResponse,
} from '../../../core/models/program.models';

/**
 * Admin program-CRUD client. Hits the ProgramController endpoints which
 * use the unconventional capitalised path segments (e.g. "GetPrograms").
 */
@Injectable({ providedIn: 'root' })
export class AdminProgramService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/program`;

  /** GET /api/v1/program/GetPrograms */
  getAll(): Observable<GetProgram[]> {
    return this.http.get<GetProgram[]>(`${this.base}/GetPrograms`)
      .pipe(catchError(this.toError));
  }

  /**
   * POST /api/v1/program/FilterPrograms.
   * Note the backend uses POST + body for filtering (not GET + query).
   * Status comes through as a string ("true"/"false") per FilterProgramsDto.
   */
  filter(req: FilterProgramsRequest): Observable<GetProgram[]> {
    return this.http.post<GetProgram[]>(`${this.base}/FilterPrograms`, req)
      .pipe(catchError(this.toError));
  }

  /** POST /api/v1/program/CreateProgram */
  create(req: CreateProgramRequest): Observable<CreateProgramResponse> {
    return this.http.post<CreateProgramResponse>(`${this.base}/CreateProgram`, req)
      .pipe(catchError(this.toError));
  }

  /** PUT /api/v1/program/UpdateProgram/{id} */
  update(id: number, req: UpdateProgramRequest): Observable<UpdateProgramResponse> {
    return this.http.put<UpdateProgramResponse>(`${this.base}/UpdateProgram/${id}`, req)
      .pipe(catchError(this.toError));
  }

  /** DELETE /api/v1/program/DeleteProgram/{id} */
  delete(id: number): Observable<DeleteProgramResponse> {
    return this.http.delete<DeleteProgramResponse>(`${this.base}/DeleteProgram/${id}`)
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
