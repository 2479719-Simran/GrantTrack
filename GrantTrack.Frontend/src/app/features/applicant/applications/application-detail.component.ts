import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { AppDocument, DOC_TYPES } from '../../../core/models/document.models';
import { GrantReport, GrantReportRequest } from '../../../core/models/grant-report.models';
import {
  DocumentApiService,
  UploadEvent,
} from '../../../core/services/document-api.service';
import { GrantReportApiService } from '../../../core/services/grant-report-api.service';
import {
  ApplicantService,
  CachedApplication,
} from '../services/applicant.service';
import { ToastService } from '../../../shared/services/toast.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { FundingPanelComponent } from '../../../shared/components/funding-panel/funding-panel.component';
import { GrantReportFormModalComponent } from './grant-report-form-modal.component';

/**
 * Detail view for a single application. The applicant uploads the
 * documents required by the program here, reviews them, and then submits.
 *
 * The list of documents comes from the BE; the application's high-level
 * info (program name, status, dates) comes from the local applicant cache
 * because the BE doesn't yet expose GET /applications/{id}.
 */
@Component({
  selector: 'app-application-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe, RouterLink, PageHeaderComponent, FundingPanelComponent],
  templateUrl: './application-detail.component.html',
  styleUrl: './application-detail.component.css',
})
export class ApplicationDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly applicantApi = inject(ApplicantService);
  private readonly docApi = inject(DocumentApiService);
  private readonly grantReportApi = inject(GrantReportApiService);
  private readonly modal = inject(NgbModal);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmDialogService);

  // Inputs
  protected readonly applicationId = signal<number>(0);

  // Cached app (program name, status, dates) â€” pulled from local cache.
  protected readonly app = computed<CachedApplication | null>(() => {
    const id = this.applicationId();
    return this.applicantApi.applications().find(a => a.applicationId === id) ?? null;
  });

  // Document state
  protected readonly documents = signal<AppDocument[]>([]);
  protected readonly loadingDocs = signal(false);
  protected readonly loadError = signal<string | null>(null);

  // Upload form state
  protected readonly docType = signal<string>('Project Proposal');
  protected readonly selectedFile = signal<File | null>(null);
  protected readonly uploadPercent = signal<number>(0);
  protected readonly uploading = signal(false);
  protected readonly dragOver = signal(false);

  // Submit state
  protected readonly submitting = signal(false);

  protected readonly docTypes = DOC_TYPES;

  // Derived UI flags
  protected readonly isDraft = computed(() => this.app()?.status === 'Draft');
  protected readonly canSubmit = computed(() =>
    this.isDraft() && this.documents().length > 0 && !this.submitting());

  // ---------- Grant Report state -----------------------------------------
  // Visible only once the application is Approved. The applicant submits a
  // utilisation report that the compliance officer reviews; the report is
  // the prerequisite for finalising any compliance check.
  protected readonly grantReport = signal<GrantReport | null>(null);
  protected readonly loadingReport = signal(false);
  protected readonly reportError = signal<string | null>(null);

  protected readonly isApproved = computed(() => this.app()?.status === 'Approved');
  /** True when the report was Returned by compliance â€” the applicant
   *  needs to fix things and resubmit. */
  protected readonly reportReturned = computed(() =>
    this.grantReport()?.status === 'Returned');
  /** True when the report has been Verified â€” locked, can't be modified. */
  protected readonly reportVerified = computed(() =>
    this.grantReport()?.status === 'Verified');

  ngOnInit(): void {
    const idParam = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isFinite(idParam) || idParam <= 0) {
      this.router.navigateByUrl('/applicant/applications');
      return;
    }
    this.applicationId.set(idParam);
    // Pull the live status from the server so the local cache reflects any
    // downstream state changes (e.g. the Approver's decision flipping the
    // status from Submitted to Approved). The Grant Report card depends on
    // isApproved() â€” which reads from the cache â€” so this refresh is what
    // makes the card appear without needing the user to re-submit.
    this.applicantApi.refreshFromServer(idParam).subscribe({
      error: (err: Error) => {
        // Surface the failure so the user knows the page is showing cached
        // data â€” the most common cause is the BE not having been restarted
        // after the new GET /api/v1/applications/{id} route was added.
        this.toast.warning(
          `Could not refresh status: ${err.message}. Showing cached value.`,
        );
        // eslint-disable-next-line no-console
        console.warn('[ApplicationDetail] status refresh failed:', err.message);
      },
    });
    this.refreshDocs();
    this.refreshGrantReport();
  }

  // ---------- Grant Report -----------------------------------------------

  refreshGrantReport(): void {
    this.loadingReport.set(true);
    this.reportError.set(null);
    this.grantReportApi.getByApplication(this.applicationId()).subscribe({
      next: report => {
        this.grantReport.set(report);
        this.loadingReport.set(false);
      },
      error: (err: Error) => {
        // 404 / "no report" cases are translated to null in the service â€”
        // anything that surfaces here is a real error worth showing.
        this.reportError.set(err.message);
        this.loadingReport.set(false);
      },
    });
  }

  openGrantReportModal(): void {
    if (this.reportVerified()) {
      this.toast.info('This report has been verified and cannot be modified.');
      return;
    }
    const ref = this.modal.open(GrantReportFormModalComponent, {
      centered: true, size: 'lg', backdrop: 'static', scrollable: true,
    });
    ref.componentInstance.applicationId = this.applicationId();
    ref.componentInstance.programName = this.app()?.programName ?? '';
    ref.componentInstance.existing = this.grantReport();

    ref.result.then(
      (payload?: GrantReportRequest) => {
        if (!payload) return;
        this.grantReportApi.upsert(payload).subscribe({
          next: saved => {
            this.grantReport.set(saved);
            this.toast.success(this.grantReport()?.status === 'Submitted'
              ? 'Grant report submitted for review.'
              : 'Grant report saved.');
          },
          error: (err: Error) => this.toast.error(err.message),
        });
      },
      () => { /* dismissed */ },
    );
  }

  protected reportStatusClass(status: string | undefined | null): string {
    switch (status) {
      case 'Verified':  return 'bg-success-subtle text-success-emphasis';
      case 'Submitted': return 'bg-info-subtle text-info-emphasis';
      case 'Returned':  return 'bg-danger-subtle text-danger-emphasis';
      case 'Draft':     return 'bg-secondary-subtle text-secondary-emphasis';
      default:          return 'bg-secondary-subtle text-secondary-emphasis';
    }
  }
  protected reportStatusIcon(status: string | undefined | null): string {
    switch (status) {
      case 'Verified':  return 'bi-check-circle';
      case 'Submitted': return 'bi-send-check';
      case 'Returned':  return 'bi-arrow-counterclockwise';
      case 'Draft':     return 'bi-file-earmark-text';
      default:          return 'bi-file-earmark';
    }
  }

  // ---------- Documents ----------------------------------------------------

  refreshDocs(): void {
    this.loadingDocs.set(true);
    this.loadError.set(null);
    this.docApi.list(this.applicationId()).subscribe({
      next: list => {
        this.documents.set(list);
        this.loadingDocs.set(false);
      },
      error: (err: Error) => {
        this.loadError.set(err.message);
        this.loadingDocs.set(false);
      },
    });
  }

  // File picker handlers
  onFilePicked(input: HTMLInputElement): void {
    const f = input.files && input.files[0];
    this.selectedFile.set(f ?? null);
    // Reset the input so picking the same file again still fires change.
    input.value = '';
  }

  onDrop(ev: DragEvent): void {
    ev.preventDefault();
    this.dragOver.set(false);
    const f = ev.dataTransfer?.files[0];
    if (f) this.selectedFile.set(f);
  }
  onDragOver(ev: DragEvent): void { ev.preventDefault(); this.dragOver.set(true); }
  onDragLeave(): void { this.dragOver.set(false); }

  uploadSelected(): void {
    const file = this.selectedFile();
    if (!file) return;

    this.uploading.set(true);
    this.uploadPercent.set(0);

    this.docApi.upload(this.applicationId(), file, this.docType()).subscribe({
      next: (e: UploadEvent) => {
        if (e.kind === 'progress') {
          this.uploadPercent.set(e.percent);
        } else {
          this.documents.update(list => [...list, e.document]);
          this.toast.success(`Uploaded "${e.document.fileName}".`);
          this.selectedFile.set(null);
          this.uploadPercent.set(0);
          this.uploading.set(false);
        }
      },
      error: (err: Error) => {
        this.uploading.set(false);
        this.uploadPercent.set(0);
        this.toast.error(err.message);
      },
    });
  }

  async deleteDoc(doc: AppDocument): Promise<void> {
    const ok = await this.confirm.confirm({
      title: 'Delete document?',
      message: `<strong>${doc.fileName}</strong> will be removed from this application. This cannot be undone.`,
      confirmText: 'Delete',
      variant: 'danger',
      icon: 'bi-trash',
    });
    if (!ok) return;

    this.docApi.delete(this.applicationId(), doc.documentId).subscribe({
      next: () => {
        this.documents.update(list => list.filter(d => d.documentId !== doc.documentId));
        this.toast.success('Document removed.');
      },
      error: (err: Error) => this.toast.error(err.message),
    });
  }

  download(doc: AppDocument): void {
    this.docApi.download(doc).subscribe({
      next: () => { /* file is on the way */ },
      error: (err: Error) => this.toast.error(err.message),
    });
  }

  clearSelected(): void { this.selectedFile.set(null); }

  // ---------- Submit -------------------------------------------------------

  submitApplication(): void {
    if (!this.isDraft()) return;
    this.submitting.set(true);
    this.applicantApi.submit(this.applicationId()).subscribe({
      next: res => {
        this.submitting.set(false);
        this.toast.success(`Application submitted (status: ${res.status}).`);
      },
      error: (err: Error) => {
        this.submitting.set(false);
        this.toast.error(err.message);
      },
    });
  }

  // ---------- Helpers ------------------------------------------------------

  protected statusClass(status: string | undefined): string {
    switch (status) {
      case 'Draft':       return 'bg-secondary-subtle text-secondary-emphasis';
      case 'Submitted':   return 'bg-info-subtle text-info-emphasis';
      case 'UnderReview': return 'bg-warning-subtle text-warning-emphasis';
      case 'Approved':    return 'bg-success-subtle text-success-emphasis';
      case 'Rejected':    return 'bg-danger-subtle text-danger-emphasis';
      default:            return 'bg-secondary-subtle text-secondary-emphasis';
    }
  }

  protected fileIcon(name: string): string {
    const ext = name.split('.').pop()?.toLowerCase() ?? '';
    if (['pdf'].includes(ext)) return 'bi-file-earmark-pdf';
    if (['jpg', 'jpeg', 'png', 'gif'].includes(ext)) return 'bi-file-earmark-image';
    if (['doc', 'docx'].includes(ext)) return 'bi-file-earmark-word';
    if (['xls', 'xlsx'].includes(ext)) return 'bi-file-earmark-excel';
    if (['txt'].includes(ext)) return 'bi-file-earmark-text';
    return 'bi-file-earmark';
  }

  protected formatBytes(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }
}
