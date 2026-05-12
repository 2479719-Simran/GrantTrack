import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AwaitingDecision } from '../../../core/models/approver.models';
import { ApproverService } from '../services/approver.service';
import { ToastService } from '../../../shared/services/toast.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

/**
 * Approver's queue page. Lists every application that has at least one
 * recommendation but no Decision yet. Each row links into the detail page
 * where the approver reviews docs + recommendations and records their call.
 */
@Component({
  selector: 'app-decisions-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DatePipe,
    DecimalPipe,
    RouterLink,
    PageHeaderComponent,
  ],
  templateUrl: './decisions-list.component.html',
  styleUrl: './decisions-list.component.css',
})
export class DecisionsListComponent implements OnInit {
  private readonly api = inject(ApproverService);
  private readonly toast = inject(ToastService);

  protected readonly rows = signal<AwaitingDecision[]>([]);
  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);

  protected readonly search = signal('');

  protected readonly visible = computed<AwaitingDecision[]>(() => {
    const q = this.search().trim().toLowerCase();
    if (!q) return this.rows();
    return this.rows().filter(r =>
      `${r.applicantName} ${r.applicantEmail} ${r.programName}`.toLowerCase().includes(q));
  });

  ngOnInit(): void { this.refresh(); }

  refresh(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.api.awaiting().subscribe({
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

  protected initials(name: string, email: string): string {
    const source = (name && name.trim()) || (email?.split('@')[0] ?? '?');
    const parts = source.split(/[\s._-]+/).filter(Boolean);
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
    return source.slice(0, 2).toUpperCase();
  }

  /** Verdict badge based on reviewer recommendations. */
  protected verdictHint(r: AwaitingDecision): { label: string; cls: string } {
    if (r.approvedCount > r.rejectedCount) {
      return { label: `${r.approvedCount} approve Â· ${r.rejectedCount} reject`, cls: 'bg-success-subtle text-success-emphasis' };
    }
    if (r.rejectedCount > r.approvedCount) {
      return { label: `${r.rejectedCount} reject Â· ${r.approvedCount} approve`, cls: 'bg-danger-subtle text-danger-emphasis' };
    }
    return { label: `${r.approvedCount} approve Â· ${r.rejectedCount} reject`, cls: 'bg-warning-subtle text-warning-emphasis' };
  }
}
