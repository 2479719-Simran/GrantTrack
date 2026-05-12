import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';

/**
 * Approver-only routes. Lazy-loaded under `/approver/*`.
 *   /approver/decisions       → queue of pending applications
 *   /approver/decisions/:id   → single application: recs + docs + decide
 */
export const APPROVER_ROUTES: Routes = [
  {
    path: '',
    canActivate: [authGuard, roleGuard('Approver')],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'decisions' },
      {
        path: 'decisions',
        loadComponent: () =>
          import('./decisions/decisions-list.component').then(m => m.DecisionsListComponent),
        title: 'Pending decisions · GrantTrack',
      },
      {
        path: 'decisions/:id',
        loadComponent: () =>
          import('./decisions/decision-detail.component').then(m => m.DecisionDetailComponent),
        title: 'Decide application · GrantTrack',
      },
    ],
  },
];
