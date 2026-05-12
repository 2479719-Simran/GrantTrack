import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  UpdateUserRequest,
  UpdateUserResponse,
  ViewUser,
} from '../../../core/models/user.models';
import { RegisterUserRequest } from '../../../core/models/auth.models';

/**
 * Admin user-management API client. Wraps the four user endpoints exposed
 * to the Admin role: list, create (via the public register endpoint), update,
 * and soft-delete (deactivate).
 */
@Injectable({ providedIn: 'root' })
export class AdminUserService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiUrl;

  /** GET /api/v1/user — list every user. Admin only. */
  getAll(): Observable<ViewUser[]> {
    return this.http
      .get<ViewUser[]>(`${this.base}/user`)
      .pipe(catchError(this.toError));
  }

  /**
   * POST /api/v1/user/registeruser — also used to create new accounts as
   * admin. Note: the backend blocks "Admin" role at this endpoint (see
   * UserService.RegisterUserAsync line 122). To make someone an admin,
   * register them as e.g. Applicant and then call `update()` to promote.
   */
  create(req: RegisterUserRequest): Observable<void> {
    return this.http
      .post(`${this.base}/user/registeruser`, req, { responseType: 'text' as const })
      .pipe(map(() => void 0), catchError(this.toError));
  }

  /**
   * POST /api/v1/user/update/{id}.
   * UserService.UpdateUser validates ALL fields — Name, Email, Phone, Role
   * and Status are all required. The view DTO doesn't return phone, so the
   * caller has to ask the admin to re-enter it.
   */
  update(id: number, req: UpdateUserRequest): Observable<UpdateUserResponse> {
    return this.http
      .post<UpdateUserResponse>(`${this.base}/user/update/${id}`, req)
      .pipe(catchError(this.toError));
  }

  /** PATCH /api/v1/user/delete-user/{id} — soft delete (Status = false). */
  deactivate(id: number): Observable<{ message: string }> {
    return this.http
      .patch<{ message: string }>(`${this.base}/user/delete-user/${id}`, {})
      .pipe(catchError(this.toError));
  }

  /** Pull the friendliest error message we can find out of the response. */
  private toError(err: HttpErrorResponse) {
    const msg = (typeof err.error === 'string' && err.error)
      || err.error?.error
      || err.error?.message
      || (err.error?.errors && Object.values<string[]>(err.error.errors)[0]?.[0])
      || `Request failed (HTTP ${err.status}).`;
    return throwError(() => new Error(msg));
  }
}
