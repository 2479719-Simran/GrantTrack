import {
  HttpErrorResponse,
  HttpInterceptorFn,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { TokenService } from '../services/token.service';

/**
 * Functional HTTP interceptor (Angular 16+). Attaches the JWT bearer token
 * to outgoing API requests, and on a 401 response clears the session and
 * sends the user back to /auth/login. We deliberately leave non-API
 * requests (assets, etc.) untouched.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const auth = inject(AuthService);
  const router = inject(Router);

  const token = tokenService.getToken();
  // Don't blanket-attach to every URL — only to our own API hosts/paths.
  const shouldAttach =
    !!token && /\/(api|notificationHub)/.test(req.url);

  const authedReq = shouldAttach
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authedReq).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401 && shouldAttach) {
        // Token rejected (expired/revoked). Boot to login and let the
        // returnUrl bring us back after re-auth.
        auth.logout(null);
        router.navigate(['/auth/login'], {
          queryParams: { returnUrl: router.url },
        });
      }
      return throwError(() => err);
    }),
  );
};
