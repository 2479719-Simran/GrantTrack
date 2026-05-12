import {
  Component,
  Input,
  OnChanges,
  SimpleChanges,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { FinanceDisbursementInfo } from '../../../core/models/finance.models';
import { ApplicationDisbursementsService } from '../../../core/services/application-disbursements.service';

/**
 * Read-only "Funding" panel â€” drops into any application detail page that
 * has an applicationId. Renders disbursement tranches with their payment
 * history, the running totals, and per-tranche progress bars.
 *
 * Used today by:
 *   â€¢ /admin/applications/:id   â€” admin sees every application's funding
 *   â€¢ /applicant/applications/:id â€” applicant sees their own funding
 *
 * No actions are exposed here â€” Schedule / Edit / Cancel / Record-payment
 * live only on the Finance Officer's drill-in page.
 */
@Component({
  selector: 'app-funding-panel',
  standalone: true,
  imports: [CommonModule, DatePipe, DecimalPipe],
  templateUrl: './funding-panel.component.html',
  styleUrl: './funding-panel.component.css',
})
export class FundingPanelComponent implements OnChanges {
  private readonly api = inject(ApplicationDisbursementsService);

  /** Application to load the funding state for. */
  @Input({ required: true }) applicationId!: number;
  /** Optional â€” when known, pass the program budget so we can render headroom. */
  @Input() programBudget: number | null = null;

  protected readonly disbursements = signal<FinanceDisbursementInfo[]>([]);
  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);

  // Money rollups derived from the loaded list.
  protected readonly totalDisbursed = computed(() =>
    this.disbursements().reduce((sum, d) => sum + d.amount, 0));
  protected readonly totalPaid = computed(() =>
    this.disbursements().reduce((sum, d) => sum + d.totalPaid, 0));
  protected readonly remaining = computed(() =>
    this.programBudget !== null ? this.programBudget - this.totalDisbursed() : null);

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['applicationId'] && this.applicationId > 0) {
      this.refresh();
    }
  }

  refresh(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.api.list(this.applicationId).subscribe({
      next: list => {
        this.disbursements.set(list ?? []);
        this.loading.set(false);
      },
      error: (err: Error) => {
        this.loadError.set(err.message);
        this.loading.set(false);
      },
    });
  }

  protected statusClass(status: string): string {
    switch (status) {
      case 'Pending':       return 'bg-secondary-subtle text-secondary-emphasis';
      case 'Scheduled':     return 'bg-info-subtle text-info-emphasis';
      case 'Paid':          return 'bg-success-subtle text-success-emphasis';
      case 'PartiallyPaid': return 'bg-warning-subtle text-warning-emphasis';
      case 'Cancelled':     return 'bg-danger-subtle text-danger-emphasis';
      default:              return 'bg-secondary-subtle text-secondary-emphasis';
    }
  }
  protected statusIcon(status: string): string {
    switch (status) {
      case 'Pending':       return 'bi-hourglass-split';
      case 'Scheduled':     return 'bi-calendar-event';
      case 'Paid':          return 'bi-check-circle';
      case 'PartiallyPaid': return 'bi-pie-chart';
      case 'Cancelled':     return 'bi-x-circle';
      default:              return 'bi-circle';
    }
  }

  protected paidPercent(d: FinanceDisbursementInfo): number {
    if (d.amount <= 0) return 0;
    return Math.min(100, Math.round((d.totalPaid / d.amount) * 100));
  }
}
