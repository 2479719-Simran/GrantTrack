import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { UserRole } from '../../core/models/enums';
import { AdminApplicationListRow } from '../../core/models/admin-application.models';
import { AdminApplicationService } from '../admin/services/admin-application.service';

interface Tile {
  title: string;
  blurb: string;
  icon: string;
  to: string;
  cta: string;
  role: UserRole;
}

/**
 * Authenticated landing page.
 *
 * For admins this becomes a real workflow dashboard: live stats by stage
 * pulled from /api/v1/admin/applications + a recent-activity feed. The
 * intent is to project every step of the grant lifecycle (Draft â†’
 * Submitted â†’ UnderReview â†’ Approved/Rejected) at a glance, then offer
 * the navigation tiles below.
 *
 * For other roles we render the simple welcome + role-relevant tiles
 * (no extra fetch, since they don't have admin endpoints to call).
 */
@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, DatePipe],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent implements OnInit {
  protected readonly auth = inject(AuthService);
  private readonly adminApi = inject(AdminApplicationService);

  /** Pretty-print the role: "FinanceOfficer" â†’ "Finance Officer". */
  protected readonly roleLabel = computed(() =>
    (this.auth.currentUser()?.role ?? '').replace(/([a-z])([A-Z])/g, '$1 $2'));

  /** Friendly local-part of the email so we can greet the user by name-ish. */
  protected readonly displayName = computed(() => {
    const local = (this.auth.currentUser()?.email ?? '').split('@')[0] ?? '';
    if (!local) return '';
    return local
      .split(/[._-]+/)
      .filter(Boolean)
      .map(p => p.charAt(0).toUpperCase() + p.slice(1))
      .join(' ');
  });

  protected readonly isAdmin = computed(() => this.auth.hasRole('Admin'));

  // ---------- Admin-only state -------------------------------------------

  protected readonly adminApps = signal<AdminApplicationListRow[]>([]);
  protected readonly loadingAdmin = signal(false);
  protected readonly adminError = signal<string | null>(null);

  /** Stage rollups â€” one card per status pillar of the lifecycle. */
  protected readonly stageStats = computed(() => {
    const list = this.adminApps();
    const count = (s: string) => list.filter(a => a.status === s).length;
    return {
      total:       list.length,
      drafts:      count('Draft'),
      submitted:   count('Submitted'),
      underReview: count('UnderReview'),
      approved:    list.filter(a => a.finalDecision === 'Approved').length,
      rejected:    list.filter(a => a.finalDecision === 'Rejected').length,
      // Workflow-stage rollups derived from pipeline counts.
      awaitingReviewer:    list.filter(a =>
        !a.finalDecision && a.status !== 'Draft' && a.reviewerCount === 0).length,
      awaitingRecommendation: list.filter(a =>
        !a.finalDecision && a.reviewerCount > 0 && a.recommendationCount === 0).length,
      awaitingDecision:    list.filter(a =>
        !a.finalDecision && a.recommendationCount > 0).length,
    };
  });

  /** Most recent applications (top 6) for the activity strip. */
  protected readonly recentApps = computed<AdminApplicationListRow[]>(() => {
    return [...this.adminApps()]
      .sort((a, b) => new Date(b.submittedDate).getTime() - new Date(a.submittedDate).getTime())
      .slice(0, 6);
  });

  ngOnInit(): void {
    if (this.isAdmin()) {
      this.refreshAdminFeed();
    }
  }

  refreshAdminFeed(): void {
    this.loadingAdmin.set(true);
    this.adminError.set(null);
    this.adminApi.list().subscribe({
      next: list => {
        this.adminApps.set(list ?? []);
        this.loadingAdmin.set(false);
      },
      error: (err: Error) => {
        this.adminError.set(err.message);
        this.loadingAdmin.set(false);
      },
    });
  }

  // ---------- Tiles (all roles) ------------------------------------------

  /** All possible tiles. Filtered down by role at render time. */
  private readonly allTiles: Tile[] = [
    { role: 'Admin', icon: 'bi-files', title: 'All applications',
      blurb: 'Track every application: status, reviewers, decisions.',
      to: '/admin/applications', cta: 'Open' },
    { role: 'Admin', icon: 'bi-people', title: 'Manage users',
      blurb: 'Activate, deactivate or update users and roles.',
      to: '/admin/users', cta: 'Open' },
    { role: 'Admin', icon: 'bi-collection', title: 'Manage programs',
      blurb: 'Create grant programs, set budgets and timelines.',
      to: '/admin/programs', cta: 'Open' },
    { role: 'Admin', icon: 'bi-diagram-3', title: 'Reviewer assignments',
      blurb: 'Bulk-assign reviewers to incoming applications.',
      to: '/admin/assignments', cta: 'Open' },

    { role: 'Applicant', icon: 'bi-collection', title: 'Browse programs',
      blurb: 'See active grant programs and apply to the ones that fit.',
      to: '/applicant/programs', cta: 'Browse' },
    { role: 'Applicant', icon: 'bi-file-earmark-text', title: 'My applications',
      blurb: 'Track your draft and submitted applications.',
      to: '/applicant/applications', cta: 'Open' },

    { role: 'Reviewer', icon: 'bi-list-check', title: 'Assigned reviews',
      blurb: 'Score and recommend on applications assigned to you.',
      to: '/reviewer/assigned', cta: 'Open' },

    { role: 'Approver', icon: 'bi-check2-square', title: 'Pending decisions',
      blurb: 'Approve or reject applications that completed review.',
      to: '/approver/decisions', cta: 'Open' },

    { role: 'FinanceOfficer', icon: 'bi-cash-coin', title: 'Disbursements',
      blurb: 'Schedule tranches, record payments and track release.',
      to: '/finance/applications', cta: 'Open' },

    { role: 'ComplianceOfficer', icon: 'bi-shield-check', title: 'Compliance checks',
      blurb: 'Schedule and complete financial &amp; operational checks.',
      to: '/compliance/applications', cta: 'Open' },
  ];

  /** Tiles visible to the current user only. */
  protected readonly myTiles = computed<Tile[]>(() => {
    const user = this.auth.currentUser();
    if (!user) return [];
    return this.allTiles.filter(t => t.role === user.role);
  });

  // ---------- Helpers used by the recent-activity feed -------------------

  protected statusClass(status: string): string {
    switch (status) {
      case 'Draft':       return 'bg-secondary-subtle text-secondary-emphasis';
      case 'Submitted':   return 'bg-info-subtle text-info-emphasis';
      case 'UnderReview': return 'bg-warning-subtle text-warning-emphasis';
      case 'Approved':    return 'bg-success-subtle text-success-emphasis';
      case 'Rejected':    return 'bg-danger-subtle text-danger-emphasis';
      default:            return 'bg-secondary-subtle text-secondary-emphasis';
    }
  }

  protected initials(name: string, email: string): string {
    const source = (name && name.trim()) || (email?.split('@')[0] ?? '?');
    const parts = source.split(/[\s._-]+/).filter(Boolean);
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
    return source.slice(0, 2).toUpperCase();
  }
}
