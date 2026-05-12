import { Routes } from '@angular/router';

/**
 * Public auth routes. Lazy-loaded under `/auth/*` from the root router.
 * Components are themselves lazy-loaded so the login page payload stays small.
 */
export const AUTH_ROUTES: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  {
    path: 'login',
    loadComponent: () =>
      import('./login/login.component').then(m => m.LoginComponent),
    title: 'Sign in · GrantTrack',
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./register/register.component').then(m => m.RegisterComponent),
    title: 'Register · GrantTrack',
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('./forgot-password/forgot-password.component').then(
        m => m.ForgotPasswordComponent,
      ),
    title: 'Reset password · GrantTrack',
  },
];
