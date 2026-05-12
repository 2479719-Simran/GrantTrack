import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';

/**
 * Reviewer-only routes. Lazy-loaded under `/reviewer/*`.
 *   /reviewer/assigned  → list of reviews assigned to me + submit recommendation
 */
export const REVIEWER_ROUTES: Routes = [
  {
    path: '',
    canActivate: [authGuard, roleGuard('Reviewer')],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'assigned' },
      {
        path: 'assigned',
        loadComponent: () =>
          import('./assigned/assigned-reviews.component').then(m => m.AssignedReviewsComponent),
        title: 'Assigned reviews · GrantTrack',
      },
    ],
  },
];
