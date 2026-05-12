import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';

/**
 * Admin-only routes. Lazy-loaded under `/admin/*`.
 * authGuard runs first (catches unauthenticated users), then roleGuard
 * blocks anyone whose JWT doesn't say they're an Admin.
 */
export const ADMIN_ROUTES: Routes = [
  {
    path: '',
    canActivate: [authGuard, roleGuard('Admin')],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'users' },
      {
        path: 'users',
        loadComponent: () =>
          import('./users/users-list.component').then(m => m.UsersListComponent),
        title: 'Users · GrantTrack',
      },
      {
        path: 'programs',
        loadComponent: () =>
          import('./programs/programs-list.component').then(m => m.ProgramsListComponent),
        title: 'Programs · GrantTrack',
      },
      {
        path: 'assignments',
        loadComponent: () =>
          import('./assignments/assignments.component').then(m => m.AssignmentsComponent),
        title: 'Reviewer assignments · GrantTrack',
      },
      {
        path: 'applications',
        loadComponent: () =>
          import('./applications/applications-list.component').then(m => m.AdminApplicationsListComponent),
        title: 'All applications · GrantTrack',
      },
      {
        path: 'applications/:id',
        loadComponent: () =>
          import('./applications/application-detail.component').then(m => m.AdminApplicationDetailComponent),
        title: 'Application detail · GrantTrack',
      },
    ],
  },
];
