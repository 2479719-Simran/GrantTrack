import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';

/**
 * FinanceOfficer-only routes. Lazy-loaded under `/finance/*`.
 *   /finance/applications       → queue of approved apps with tranche state
 *   /finance/applications/:id   → manage tranches + record payments
 */
export const FINANCE_ROUTES: Routes = [
  {
    path: '',
    canActivate: [authGuard, roleGuard('FinanceOfficer')],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'applications' },
      // The navbar link from earlier modules pointed at /finance/disbursements;
      // alias to the queue page so old links don't 404.
      { path: 'disbursements', pathMatch: 'full', redirectTo: 'applications' },
      {
        path: 'applications',
        loadComponent: () =>
          import('./applications/applications-list.component').then(m => m.FinanceApplicationsListComponent),
        title: 'Approved applications · GrantTrack',
      },
      {
        path: 'applications/:id',
        loadComponent: () =>
          import('./applications/application-detail.component').then(m => m.FinanceApplicationDetailComponent),
        title: 'Manage disbursements · GrantTrack',
      },
    ],
  },
];
