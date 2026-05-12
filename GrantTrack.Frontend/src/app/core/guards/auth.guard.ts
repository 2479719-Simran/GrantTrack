import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { TokenService } from '../services/token.service';

/**
 * Blocks access to a route unless a valid (non-expired) JWT is present.
 * On block, redirects to /auth/login with a returnUrl so the user lands
 * back where they were after signing in.
 */
export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const token = inject(TokenService);
  const router = inject(Router);

  if (auth.isLoggedIn() && !token.isExpired()) {
    return true;
  }

  // Stale or missing token — clear local state before bouncing.
  auth.logout(null);
  return router.createUrlTree(['/auth/login'], {
    queryParams: { returnUrl: state.url },
  });
};
