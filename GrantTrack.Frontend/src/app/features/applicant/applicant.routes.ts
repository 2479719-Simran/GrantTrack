import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';

/**
 * Applicant-only routes. Lazy-loaded under `/applicant/*`.
 *   /applicant/programs      → browse + apply
 *   /applicant/applications  → my drafts/submitted apps
 */
export const APPLICANT_ROUTES: Routes = [
  {
    path: '',
    canActivate: [authGuard, roleGuard('Applicant')],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'programs' },
      {
        path: 'programs',
        loadComponent: () =>
          import('./programs/programs-catalog.component').then(m => m.ProgramsCatalogComponent),
        title: 'Programs · GrantTrack',
      },
      {
        path: 'applications',
        loadComponent: () =>
          import('./applications/my-applications.component').then(m => m.MyApplicationsComponent),
        title: 'My applications · GrantTrack',
      },
      {
        // Detail / document-management page for a single application.
        path: 'applications/:id',
        loadComponent: () =>
          import('./applications/application-detail.component').then(m => m.ApplicationDetailComponent),
        title: 'Application · GrantTrack',
      },
    ],
  },
];
