import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../models/enums';

/**
 * Factory for role-restricted routes. Pair with `authGuard` in the route
 * definition (authGuard first to handle the unauthenticated case).
 *
 * Usage:
 *   { path: 'admin', canActivate: [authGuard, roleGuard('Admin')], ... }
 */
export const roleGuard = (...allowed: UserRole[]): CanActivateFn => {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (auth.hasRole(...allowed)) {
      return true;
    }
    // Authenticated but wrong role — kick to a neutral landing page.
    return router.createUrlTree(['/forbidden']);
  };
};
