import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AdminApplicationListRow } from '../../../core/models/admin-application.models';
import { AdminApplicationService } from '../services/admin-application.service';
import { ToastService } from '../../../shared/services/toast.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

type StatusFilter = '' | 'Draft' | 'Submitted' | 'UnderReview' | 'Approved' | 'Rejected';

/**
 * Admin's full visibility hub. Lists every application across the system
 * with applicant + program + status + pipeline rollups.
 *
 * Filtering is client-side (status + free-text search) because the dataset
 * fits comfortably in one round-trip; if it grows large we'd push these
 * to query params on the BE.
 */
@Component({
  selector: 'app-admin-applications-list',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe, RouterLink, PageHeaderComponent],
  templateUrl: './applications-list.component.html',
  styleUrl: './applications-list.component.css',
})
export class AdminApplicationsListComponent implements OnInit {
  private readonly api = inject(AdminApplicationService);
  private readonly toast = inject(ToastService);

  protected readonly rows = signal<AdminApplicationListRow[]>([]);
  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);

  protected readonly search = signal('');
  protected readonly statusFilter = signal<StatusFilter>('');
  protected readonly statuses: StatusFilter[] = ['', 'Draft', 'Submitted', 'UnderReview', 'Approved', 'Rejected'];

  protected readonly visible = computed<AdminApplicationListRow[]>(() => {
    const q = this.search().trim().toLowerCase();
    const s = this.statusFilter();
    return this.rows().filter(r => {
      if (s && r.status !== s) return false;
      if (q) {
        const hay = `${r.applicantName} ${r.applicantEmail} ${r.programName} ${r.applicationId}`.toLowerCase();
        if (!hay.includes(q)) return false;
      }
      return true;
    });
  });

  // High-level rollup â€” used by the stat strip at the top of the page.
  protected readonly totalCount = computed(() => this.rows().length);
  protected readonly draftCount = computed(() => this.rows().filter(r => r.status === 'Draft').length);
  protected readonly submittedCount = computed(() => this.rows().filter(r => r.status === 'Submitted').length);
  protected readonly approvedCount = computed(() => this.rows().filter(r => r.status === 'Approved').length);
  protected readonly rejectedCount = computed(() => this.rows().filter(r => r.status === 'Rejected').length);

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
