import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import {
  ComplianceApplicationDetail,
  ComplianceCheckInfo,
} from '../../../core/models/compliance.models';
import { ComplianceType } from '../../../core/models/enums';
import { ComplianceService } from '../services/compliance.service';
import { ComplianceCheckService } from '../services/compliance-check.service';
import { ToastService } from '../../../shared/services/toast.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { FundingPanelComponent } from '../../../shared/components/funding-panel/funding-panel.component';
import { ScheduleCheckModalComponent } from './schedule-check-modal.component';
import { CompleteCheckModalComponent } from './complete-check-modal.component';

/**
 * Compliance Officer's drill-in view for a single approved application.
 * Surfaces:
 *   â€¢ Applicant + program + decision context
 *   â€¢ Grant Report status (or its absence â€” the prerequisite for completion)
 *   â€¢ Read-only funding panel (re-using <app-funding-panel>) so the CO sees
 *     where the money has gone before signing off
 *   â€¢ Every compliance check (Scheduled / Verified / Returned) with
 *     actions â€” Schedule new check, Complete an existing one
 */
@Component({
  selector: 'app-compliance-application-detail',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    DecimalPipe,
    RouterLink,
    PageHeaderComponent,
    FundingPanelComponent,
  ],
  templateUrl: './application-detail.component.html',
  styleUrl: './application-detail.component.css',
})
export class ComplianceApplicationDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(ComplianceService);
  private readonly checkApi = inject(ComplianceCheckService);
  private readonly modal = inject(NgbModal);
  private readonly toast = inject(ToastService);

  protected readonly applicationId = signal<number>(0);
  protected readonly detail = signal<ComplianceApplicationDetail | null>(null);
  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);

  /** True when the application has a GrantReport â€” drives whether
   *  the Complete-check modal warns up-front. */
  protected readonly hasGrantReport = computed(() =>
    this.detail()?.grantReportId !== null && this.detail()?.grantReportId !== undefined);

  /** Types already scheduled â€” so the schedule modal can disable them. */
  protected readonly existingTypes = computed<ComplianceType[]>(() => {
    const list = this.detail()?.checks ?? [];
    return [...new Set(list.map(c => c.type as ComplianceType))];
  });

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isFinite(id) || id <= 0) {
      this.router.navigateByUrl('/compliance/applications');
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

  openSchedule(): void {
    const d = this.detail();
    if (!d) return;
    const ref = this.modal.open(ScheduleCheckModalComponent, {
      centered: true, size: 'lg', backdrop: 'static',
    });
    ref.componentInstance.applicationId = this.applicationId();
    ref.componentInstance.applicantName = d.applicantName;
    ref.componentInstance.existingTypes = this.existingTypes();

    ref.result.then(
      (payload?: { applicationId: number; type: ComplianceType; notes: string }) => {
        if (!payload) return;
        this.checkApi.schedule({
          applicationId: payload.applicationId,
          type: payload.type,
          notes: payload.notes,
        }).subscribe({
          next: () => {
            this.toast.success(`${payload.type} check scheduled.`);
            this.refresh();
          },
          error: (err: Error) => this.toast.error(err.message),
        });
      },
      () => { /* dismissed */ },
    );
  }

  openComplete(check: ComplianceCheckInfo): void {
    if (check.stage === 'Verified') {
      this.toast.info('This check is already verified and immutable.');
      return;
    }
    const ref = this.modal.open(CompleteCheckModalComponent, {
      centered: true, size: 'lg', backdrop: 'static',
    });
    ref.componentInstance.check = check;
    ref.componentInstance.hasGrantReport = this.hasGrantReport();

    ref.result.then(
      (payload?: { checkId: number; result: 'Completed' | 'Flagged'; notes: string }) => {
        if (!payload) return;
        this.checkApi.complete(payload.checkId, {
          result: payload.result,
          notes: payload.notes,
        }).subscribe({
          next: () => {
            this.toast.success(`Check #${payload.checkId} finalised as ${payload.result}.`);
            this.refresh();
          },
          error: (err: Error) => this.toast.error(err.message),
        });
      },
      () => { /* dismissed */ },
    );
  }

  // ---------- helpers ----------------------------------------------------

  protected stageClass(stage: string): string {
    switch (stage) {
      case 'Verified': return 'bg-success-subtle text-success-emphasis';
      case 'Returned': return 'bg-danger-subtle text-danger-emphasis';
      case 'Scheduled':
      default:         return 'bg-warning-subtle text-warning-emphasis';
    }
  }
  protected stageIcon(stage: string): string {
    switch (stage) {
      case 'Verified': return 'bi-check-circle';
      case 'Returned': return 'bi-flag-fill';
      case 'Scheduled':
      default:         return 'bi-hourglass-split';
    }
  }

  protected typeIcon(type: string): string {
    return type === 'Financial' ? 'bi-cash-stack' : 'bi-gear';
  }

  protected reportStatusClass(status: string | null | undefined): string {
    switch (status) {
      case 'Verified':  return 'bg-success-subtle text-success-emphasis';
      case 'Submitted': return 'bg-info-subtle text-info-emphasis';
      case 'Returned':  return 'bg-danger-subtle text-danger-emphasis';
      case 'Draft':     return 'bg-secondary-subtle text-secondary-emphasis';
      default:          return 'bg-secondary-subtle text-secondary-emphasis';
    }
  }

  protected initials(name: string, email: string): string {
    const source = (name && name.trim()) || (email?.split('@')[0] ?? '?');
    const parts = source.split(/[\s._-]+/).filter(Boolean);
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
    return source.slice(0, 2).toUpperCase();
  }
}
