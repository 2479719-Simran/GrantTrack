import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import {
  FinanceApplicationDetail,
  FinanceDisbursementInfo,
} from '../../../core/models/finance.models';
import { FinanceService } from '../services/finance.service';
import { DisbursementService } from '../services/disbursement.service';
import { ToastService } from '../../../shared/services/toast.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { DisbursementFormModalComponent } from './disbursement-form-modal.component';
import { PaymentFormModalComponent } from './payment-form-modal.component';

/**
 * Finance Officer's drill-in view for one approved application. The page
 * lets the FO:
 *   â€¢ Read the approver's decision context (who + when + notes)
 *   â€¢ Create new disbursement tranches up to the program-budget headroom
 *   â€¢ Update an existing tranche (amount, dates, status) â€” within BE rules
 *   â€¢ Record payments against Scheduled tranches
 *
 * After every successful mutation we re-fetch the detail so totals stay
 * accurate (the BE does the heavy lifting; we trust its rollups).
 */
@Component({
  selector: 'app-finance-application-detail',
  standalone: true,
  imports: [CommonModule, DatePipe, DecimalPipe, RouterLink, PageHeaderComponent],
  templateUrl: './application-detail.component.html',
  styleUrl: './application-detail.component.css',
})
export class FinanceApplicationDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(FinanceService);
  private readonly disbApi = inject(DisbursementService);
  private readonly modal = inject(NgbModal);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmDialogService);

  protected readonly applicationId = signal<number>(0);
  protected readonly detail = signal<FinanceApplicationDetail | null>(null);
  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isFinite(id) || id <= 0) {
      this.router.navigateByUrl('/finance/applications');
      return;
    }
    this.applicationId.set(id);
    this.refresh();
  }

  refresh(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.api.getDetail(this.applicationId()).subscribe({
      next: d => {
        this.detail.set(d);
        this.loading.set(false);
      },
      error: (err: Error) => {
        this.loadError.set(err.message);
        this.loading.set(false);
      },
    });
  }

  // ---------- Disbursement actions ---------------------------------------

  openCreateDisbursement(): void {
    const d = this.detail();
    if (!d) return;
    const ref = this.modal.open(DisbursementFormModalComponent, {
      centered: true, size: 'lg', backdrop: 'static',
    });
    ref.componentInstance.disbursement = null;
    ref.componentInstance.remainingBudget = d.remaining;

    ref.result.then(
      (payload?: { amount: number; scheduledDate: string }) => {
        if (!payload) return;
        this.disbApi.create({
          applicationId: this.applicationId(),
          amount: payload.amount,
          scheduledDate: payload.scheduledDate,
        }).subscribe({
          next: () => {
            this.toast.success(`Tranche of â‚¹ ${payload.amount} created.`);
            this.refresh();
          },
          error: (err: Error) => this.toast.error(err.message),
        });
      },
      () => { /* dismissed */ },
    );
  }

  openUpdateDisbursement(disb: FinanceDisbursementInfo): void {
    if (this.isTerminalStatus(disb.status)) {
      this.toast.info('This tranche is finalised and can no longer be modified.');
      return;
    }
    const d = this.detail();
    if (!d) return;
    const ref = this.modal.open(DisbursementFormModalComponent, {
      centered: true, size: 'lg', backdrop: 'static',
    });
    ref.componentInstance.disbursement = disb;
    // Edits to amount need the same headroom rule, but we have to add the
    // current tranche's own amount back in (since the BE does this dance
    // server-side anyway â€” see DisbursementService.UpdateDisbursementAsync).
    ref.componentInstance.remainingBudget = d.remaining + disb.amount;

    ref.result.then(
      (changes?: Record<string, unknown>) => {
        if (!changes || Object.keys(changes).length === 0) {
          this.toast.info('No changes to save.');
          return;
        }
        this.disbApi.update(disb.disbursementId, changes).subscribe({
          next: () => {
            this.toast.success(`Tranche #${disb.disbursementId} updated.`);
            this.refresh();
          },
          error: (err: Error) => this.toast.error(err.message),
        });
      },
      () => { /* dismissed */ },
    );
  }

  /** Quick-action to flip a Pending tranche to Scheduled without opening the modal. */
  async scheduleTranche(disb: FinanceDisbursementInfo): Promise<void> {
    if (disb.status !== 'Pending') return;
    const ok = await this.confirm.confirm({
      title: 'Schedule this tranche?',
      message: `Tranche <strong>#${disb.disbursementId}</strong> will move to <strong>Scheduled</strong> and will then accept payments.`,
      confirmText: 'Schedule',
      variant: 'primary',
      icon: 'bi-calendar-check',
    });
    if (!ok) return;
    this.disbApi.update(disb.disbursementId, { status: 'Scheduled' }).subscribe({
      next: () => {
        this.toast.success(`Tranche #${disb.disbursementId} scheduled.`);
        this.refresh();
      },
      error: (err: Error) => this.toast.error(err.message),
    });
  }

  async cancelTranche(disb: FinanceDisbursementInfo): Promise<void> {
    if (this.isTerminalStatus(disb.status)) return;
    const ok = await this.confirm.confirm({
      title: 'Cancel this tranche?',
      message: `Tranche <strong>#${disb.disbursementId}</strong> will be cancelled and can no longer be paid against. This frees up â‚¹ ${disb.amount} of program budget.`,
      confirmText: 'Cancel tranche',
      variant: 'danger',
      icon: 'bi-x-circle',
    });
    if (!ok) return;
    this.disbApi.update(disb.disbursementId, { status: 'Cancelled' }).subscribe({
      next: () => {
        this.toast.success(`Tranche #${disb.disbursementId} cancelled.`);
        this.refresh();
      },
      error: (err: Error) => this.toast.error(err.message),
    });
  }

  // ---------- Payment actions --------------------------------------------

  openRecordPayment(disb: FinanceDisbursementInfo): void {
    if (disb.status !== 'Scheduled') {
      this.toast.info('Payments can only be recorded against a Scheduled tranche.');
      return;
    }
    if (disb.remainingOnTranche <= 0) {
      this.toast.info('This tranche has no remaining balance.');
      return;
    }
    const ref = this.modal.open(PaymentFormModalComponent, {
      centered: true, size: 'lg', backdrop: 'static',
    });
    ref.componentInstance.disbursement = disb;

    ref.result.then(
      (payload?: { disbursementId: number; amount: number; date: string; method: string }) => {
        if (!payload) return;
        this.disbApi.recordPayment({
          disbursementId: payload.disbursementId,
          amount: payload.amount,
          date: payload.date,
          method: payload.method as any,
        }).subscribe({
          next: () => {
            this.toast.success(`Payment of â‚¹ ${payload.amount} recorded.`);
            this.refresh();
          },
          error: (err: Error) => this.toast.error(err.message),
        });
      },
      () => { /* dismissed */ },
    );
  }

  // ---------- helpers ----------------------------------------------------

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

  protected isTerminalStatus(status: string): boolean {
    return status === 'Paid' || status === 'Cancelled';
  }

  protected paidPercent(disb: FinanceDisbursementInfo): number {
    if (disb.amount <= 0) return 0;
    return Math.min(100, Math.round((disb.totalPaid / disb.amount) * 100));
  }

  protected initials(name: string, email: string): string {
    const source = (name && name.trim()) || (email?.split('@')[0] ?? '?');
    const parts = source.split(/[\s._-]+/).filter(Boolean);
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
    return source.slice(0, 2).toUpperCase();
  }
}
