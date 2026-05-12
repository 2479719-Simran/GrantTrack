import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { FinanceApplicationListRow } from '../../../core/models/finance.models';
import { FinanceService } from '../services/finance.service';
import { ToastService } from '../../../shared/services/toast.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

type FilterTab = 'all' | 'no-tranches' | 'in-flight' | 'fully-paid';

/**
 * Finance Officer's queue page. Lists every approved application with
 * pipeline rollups so the FO can quickly see which need new tranches,
 * which have payments outstanding, and which are fully paid.
 *
 * Tab definitions (client-side filter â€” the BE returns the full set):
 *   â€¢ All          â€” every approved application
 *   â€¢ Needs setup  â€” disbursement count == 0
 *   â€¢ In flight    â€” has tranches, but totalPaid < totalDisbursed
 *   â€¢ Fully paid   â€” totalPaid >= totalDisbursed and there's at least one tranche
 */
@Component({
  selector: 'app-finance-applications-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DatePipe,
    DecimalPipe,
    RouterLink,
    PageHeaderComponent,
  ],
  templateUrl: './applications-list.component.html',
  styleUrl: './applications-list.component.css',
})
export class FinanceApplicationsListComponent implements OnInit {
  private readonly api = inject(FinanceService);
  private readonly toast = inject(ToastService);

  protected readonly rows = signal<FinanceApplicationListRow[]>([]);
  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);

  protected readonly search = signal('');
  protected readonly tab = signal<FilterTab>('all');

  protected readonly visible = computed<FinanceApplicationListRow[]>(() => {
    const q = this.search().trim().toLowerCase();
    const filterTab = this.tab();
    return this.rows().filter(r => {
      if (q) {
        const hay = `${r.applicantName} ${r.applicantEmail} ${r.programName} ${r.applicationId}`.toLowerCase();
        if (!hay.includes(q)) return false;
      }
      switch (filterTab) {
        case 'no-tranches':
          return r.disbursementCount === 0;
        case 'in-flight':
          return r.disbursementCount > 0 && r.totalPaid < r.totalDisbursed;
        case 'fully-paid':
          return r.disbursementCount > 0 && r.totalPaid >= r.totalDisbursed;
        default:
          return true;
      }
    });
  });

  // Stat-strip rollups â€” same definitions as the tabs.
  protected readonly totalCount = computed(() => this.rows().length);
  protected readonly noTranchesCount = computed(() =>
    this.rows().filter(r => r.disbursementCount === 0).length);
  protected readonly inFlightCount = computed(() =>
    this.rows().filter(r => r.disbursementCount > 0 && r.totalPaid < r.totalDisbursed).length);
  protected readonly fullyPaidCount = computed(() =>
    this.rows().filter(r => r.disbursementCount > 0 && r.totalPaid >= r.totalDisbursed).length);

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

  setTab(t: FilterTab): void {
    if (this.tab() === t) return;
    this.tab.set(t);
  }

  protected initials(name: string, email: string): string {
    const source = (name && name.trim()) || (email?.split('@')[0] ?? '?');
    const parts = source.split(/[\s._-]+/).filter(Boolean);
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
    return source.slice(0, 2).toUpperCase();
  }

  /** Fraction paid out of total disbursed â€” used to drive the progress bar. */
  protected paidPercent(r: FinanceApplicationListRow): number {
    if (r.totalDisbursed <= 0) return 0;
    return Math.min(100, Math.round((r.totalPaid / r.totalDisbursed) * 100));
  }
}
