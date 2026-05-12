import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ComplianceApplicationListRow } from '../../../core/models/compliance.models';
import { ComplianceService } from '../services/compliance.service';
import { ToastService } from '../../../shared/services/toast.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

type Tab = 'all' | 'no-checks' | 'pending' | 'verified' | 'flagged';

/**
 * Compliance Officer's queue page. Lists every approved application with
 * its compliance state â€” number + outcomes of checks, and the grantee's
 * GrantReport status.
 *
 * Tabs:
 *   â€¢ All        â€” every approved application
 *   â€¢ Needs check â€” no compliance check scheduled yet
 *   â€¢ Pending    â€” has at least one check that's still Scheduled
 *   â€¢ Verified   â€” Grant Report is Verified (clean compliance)
 *   â€¢ Flagged    â€” at least one check returned with Flagged status
 */
@Component({
  selector: 'app-compliance-applications-list',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe, RouterLink, PageHeaderComponent],
  templateUrl: './applications-list.component.html',
  styleUrl: './applications-list.component.css',
})
export class ComplianceApplicationsListComponent implements OnInit {
  private readonly api = inject(ComplianceService);
  private readonly toast = inject(ToastService);

  protected readonly rows = signal<ComplianceApplicationListRow[]>([]);
  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);

  protected readonly search = signal('');
  protected readonly tab = signal<Tab>('all');

  protected readonly visible = computed<ComplianceApplicationListRow[]>(() => {
    const q = this.search().trim().toLowerCase();
    const filterTab = this.tab();
    return this.rows().filter(r => {
      if (q) {
        const hay = `${r.applicantName} ${r.applicantEmail} ${r.programName} ${r.applicationId}`.toLowerCase();
        if (!hay.includes(q)) return false;
      }
      switch (filterTab) {
        case 'no-checks':
          return r.checkCount === 0;
        case 'pending':
          return r.scheduledCount > 0;
        case 'verified':
          return r.reportStatus === 'Verified';
        case 'flagged':
          return r.flaggedCount > 0 || r.reportStatus === 'Returned';
        default:
          return true;
      }
    });
  });

  // Stat-strip rollups â€” same definitions as the tabs.
  protected readonly totalCount = computed(() => this.rows().length);
  protected readonly noChecksCount = computed(() =>
    this.rows().filter(r => r.checkCount === 0).length);
  protected readonly pendingCount = computed(() =>
    this.rows().filter(r => r.scheduledCount > 0).length);
  protected readonly verifiedCount = computed(() =>
    this.rows().filter(r => r.reportStatus === 'Verified').length);
  protected readonly flaggedCount = computed(() =>
    this.rows().filter(r => r.flaggedCount > 0 || r.reportStatus === 'Returned').length);

  ngOnInit(): void { this.refresh(); }

  refresh(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.api.list().subscribe({
      next: list => {
        this.rows.set(list ?? []);
        this.loading.set(false);
      },
      error: (err: Error) => {
        this.loadError.set(err.message);
        this.loading.set(false);
        this.toast.error(err.message);
      },
    });
  }

  setTab(t: Tab): void {
    if (this.tab() === t) return;
    this.tab.set(t);
  }

  /** Aggregate compliance label per row for the table column. */
  protected complianceLabel(r: ComplianceApplicationListRow): { label: string; cls: string } {
    if (r.checkCount === 0) {
      return { label: 'No checks scheduled', cls: 'bg-secondary-subtle text-secondary-emphasis' };
    }
    if (r.reportStatus === 'Verified') {
      return { label: 'Verified', cls: 'bg-success-subtle text-success-emphasis' };
    }
    if (r.reportStatus === 'Returned' || r.flaggedCount > 0) {
      return { label: 'Flagged Â· returned', cls: 'bg-danger-subtle text-danger-emphasis' };
    }
    return { label: 'Awaiting review', cls: 'bg-warning-subtle text-warning-emphasis' };
  }

  protected initials(name: string, email: string): string {
    const source = (name && name.trim()) || (email?.split('@')[0] ?? '?');
    const parts = source.split(/[\s._-]+/).filter(Boolean);
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
    return source.slice(0, 2).toUpperCase();
  }
}
