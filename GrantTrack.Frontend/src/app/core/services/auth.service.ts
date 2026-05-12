import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, map, tap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CurrentUser,
  ForgotPasswordRequest,
  LoginRequest,
  LoginResponse,
  RegisterUserRequest,
} from '../models/auth.models';
import { UserRole } from '../models/enums';
import { TokenService } from './token.service';

/**
 * AuthService owns login/register/forgot-password and exposes the current
 * user as a signal so guards, navbar, and feature pages can react.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly token = inject(TokenService);
  private readonly router = inject(Router);

  /** Reactive view of the logged-in user, derived from the stored JWT. */
  private readonly _currentUser = signal<CurrentUser | null>(this.token.decode());
  readonly currentUser = this._currentUser.asReadonly();
  readonly isLoggedIn = computed(() => this._currentUser() !== null);
  readonly role = computed(() => this._currentUser()?.role ?? null);

  /**
   * POST /api/v1/user/login.
   * The backend returns the raw JWT string on success — not an envelope —
   * so we capture and persist it here.
   */
  login(req: LoginRequest): Observable<CurrentUser> {
    const url = `${environment.apiUrl}/user/login`;
    return this.http
      .post(url, req, { responseType: 'text' as const })
      .pipe(
        map((rawToken: LoginResponse) => {
          // Some servers double-quote string responses; strip a surrounding pair.
          const token = rawToken.replace(/^"|"$/g, '');
          this.token.setToken(token);
          const user = this.token.decode();
          if (!user) {
            throw new Error('Login succeeded but the token could not be decoded.');
          }
          this._currentUser.set(user);
          return user;
        }),
        catchError((err: HttpErrorResponse) => {
          // Backend returns { error: "..." } for 400/401 — surface that text.
          const message = (err.error && (err.error.error || err.error.message))
            || (typeof err.error === 'string' ? err.error : null)
            || 'Login failed. Please try again.';
          return throwError(() => new Error(message));
        }),
      );
  }

  /**
   * POST /api/v1/user/registeruser. Returns 201 with no parsed body.
   */
  register(req: RegisterUserRequest): Observable<void> {
    const url = `${environment.apiUrl}/user/registeruser`;
    return this.http.post(url, req, { responseType: 'text' as const }).pipe(
      map(() => void 0),
      catchError((err: HttpErrorResponse) => {
        // 400 → ModelState dict OR plain string; 409 → conflict string.
        const message = this.firstFieldError(err) || 'Registration failed. Please try again.';
        return throwError(() => new Error(message));
      }),
    );
  }

  /**
   * POST /api/v1/user/forgotpassword.
   * Success body: { message: "..." }; failure body: plain string.
   */
  forgotPassword(req: ForgotPasswordRequest): Observable<string> {
    const url = `${environment.apiUrl}/user/forgotpassword`;
    return this.http.post<{ message: string }>(url, req).pipe(
      map(res => res.message),
      catchError((err: HttpErrorResponse) => {
        const message = (typeof err.error === 'string' && err.error)
          || (err.error?.message ?? null)
          || 'Could not reset password.';
        return throwError(() => new Error(message));
      }),
    );
  }

  logout(navigateTo: string | null = '/auth/login'): void {
    this.token.clear();
    this._currentUser.set(null);
    if (navigateTo) {
      this.router.navigateByUrl(navigateTo);
    }
  }

  hasRole(...roles: UserRole[]): boolean {
    const r = this.role();
    return r !== null && roles.includes(r);
  }

  /**
   * Pulls a friendly first error out of an ASP.NET ModelState response
   * (`{ "field": ["msg1", "msg2"] }`), or falls back to common shapes.
   */
  private firstFieldError(err: HttpErrorResponse): string | null {
    if (!err.error) return null;
    if (typeof err.error === 'string') return err.error;
    if (err.error.error) return err.error.error;
    if (err.error.message) return err.error.message;
    if (err.error.errors && typeof err.error.errors === 'object') {
      const firstKey = Object.keys(err.error.errors)[0];
      const arr = err.error.errors[firstKey];
      if (Array.isArray(arr) && arr.length) return arr[0];
    }
    // ModelState dict at the root.
    const firstKey = Object.keys(err.error)[0];
    const val = err.error[firstKey];
    if (Array.isArray(val) && val.length) return val[0];
    return null;
  }
}
