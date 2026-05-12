import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';

/**
 * ComplianceOfficer-only routes. Lazy-loaded under `/compliance/*`.
 *   /compliance/applications       → queue of approved apps + check state
 *   /compliance/applications/:id   → manage checks for one application
 */
export const COMPLIANCE_ROUTES: Routes = [
  {
    path: '',
    canActivate: [authGuard, roleGuard('ComplianceOfficer')],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'applications' },
      // Old navbar URL — alias so existing links don't break.
      { path: 'checks', pathMatch: 'full', redirectTo: 'applications' },
      {
        path: 'applications',
        loadComponent: () =>
          import('./applications/applications-list.component')
            .then(m => m.ComplianceApplicationsListComponent),
        title: 'Compliance · GrantTrack',
      },
      {
        path: 'applications/:id',
        loadComponent: () =>
          import('./applications/application-detail.component')
            .then(m => m.ComplianceApplicationDetailComponent),
        title: 'Compliance review · GrantTrack',
      },
    ],
  },
];
