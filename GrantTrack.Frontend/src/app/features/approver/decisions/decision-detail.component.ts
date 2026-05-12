import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  ApproverRecommendation,
  AwaitingDecision,
} from '../../../core/models/approver.models';
import { AppDocument } from '../../../core/models/document.models';
import { DecisionStatus } from '../../../core/models/enums';
import { ApproverService } from '../services/approver.service';
import { DocumentApiService } from '../../../core/services/document-api.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/services/toast.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

/**
 * Single-application decision page for the Approver. Three panels:
 *
 *   1. Application summary  â€” applicant + program info
 *   2. Reviewer recommendations  â€” what each Reviewer concluded (with scores)
 *   3. Supporting documents  â€” downloadable
 *   4. Decision form  â€” Approved/Rejected + notes (max 1000 chars per BE)
 *
 * The application summary is pulled from the awaiting-list endpoint
 * (filtered to this id) because the BE has no GET /applications/{id} yet.
 */
@Component({
  selector: 'app-decision-detail',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    ReactiveFormsModule,
    RouterLink,
    PageHeaderComponent,
  ],
  templateUrl: './decision-detail.component.html',
  styleUrl: './decision-detail.component.css',
})
export class DecisionDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApproverService);
  private readonly docApi = inject(DocumentApiService);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmDialogService);

  protected readonly applicationId = signal<number>(0);

  protected readonly app = signal<AwaitingDecision | null>(null);
  protected readonly loadingApp = signal(false);
  protected readonly appLoadError = signal<string | null>(null);

  protected readonly recommendations = signal<ApproverRecommendation[]>([]);
  protected readonly loadingRecs = signal(false);

  protected readonly documents = signal<AppDocument[]>([]);
  protected readonly loadingDocs = signal(false);
  protected readonly downloadingDocId = signal<number | null>(null);

  protected readonly submitting = signal(false);

  /** Decision form. */
  protected readonly form: FormGroup = this.fb.nonNullable.group({
    decisionValue: ['Approved' as DecisionStatus, [Validators.required]],
    notes: ['', [Validators.required, Validators.maxLength(1000)]],
  });

  protected readonly approvedRecCount = computed(() =>
    this.recommendations().filter(r => r.decision === 'Approved').length);
  protected readonly rejectedRecCount = computed(() =>
    this.recommendations().filter(r => r.decision === 'Rejected').length);
  protected readonly avgScore = computed(() => {
    const scored = this.recommendations().filter(r => r.score > 0);
    if (scored.length === 0) return 0;
    return scored.reduce((s, r) => s + r.score, 0) / scored.length;
  });

  /**
   * True only when the application is still in the awaiting-decision queue.
   * The submit button binds to this so a refresh of an already-decided
   * page can't fire another POST that the BE will reject anyway.
   */
  protected readonly canDecide = computed(() =>
    !this.loadingApp() && this.app() !== null);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isFinite(id) || id <= 0) {
      this.router.navigateByUrl('/approver/decisions');
      return;
    }
    this.applicationId.set(id);
    this.loadAll();
  }

  loadAll(): void {
    this.loadAppSummary();
    this.loadRecs();
    this.loadDocs();
  }

  /**
   * Hydrate the summary card by filtering the awaiting-list to this id.
   * If the row isn't there (e.g. someone else just decided), bounce back.
   */
  private loadAppSummary(): void {
    this.loadingApp.set(true);
    this.appLoadError.set(null);
    this.api.awaiting().subscribe({
      next: list => {
        this.loadingApp.set(false);
        const match = list.find(r => r.applicationId === this.applicationId()) ?? null;
        this.app.set(match);
        if (!match) {
          this.appLoadError.set('This application is no longer in the queue. It may have already been decided.');
        }
      },
      error: (err: Error) => {
        this.loadingApp.set(false);
        this.appLoadError.set(err.message);
      },
    });
  }

  private loadRecs(): void {
    this.loadingRecs.set(true);
    this.api.recommendations(this.applicationId()).subscribe({
      next: list => {
        this.recommendations.set(list);
        this.loadingRecs.set(false);
      },
      error: (err: Error) => {
        this.loadingRecs.set(false);
        this.toast.error(`Could not load recommendations: ${err.message}`);
      },
    });
  }

  private loadDocs(): void {
    this.loadingDocs.set(true);
    this.docApi.list(this.applicationId()).subscribe({
      next: list => {
        this.documents.set(list);
        this.loadingDocs.set(false);
      },
      error: (err: Error) => {
        this.loadingDocs.set(false);
        this.toast.error(`Could not load documents: ${err.message}`);
      },
    });
  }

  download(doc: AppDocument): void {
    this.downloadingDocId.set(doc.documentId);
    this.docApi.download(doc).subscribe({
      next: () => this.downloadingDocId.set(null),
      error: (err: Error) => {
        this.downloadingDocId.set(null);
        this.toast.error(err.message);
      },
    });
  }

  setDecision(d: DecisionStatus): void {
    this.form.patchValue({ decisionValue: d });
  }

  async submit(): Promise<void> {
    // Hard stop: BE rejects re-decisions with a 400. We let the user know
    // up-front instead of waiting for the round-trip.
    if (!this.canDecide()) {
      this.toast.warning('This application has already been decided and cannot be modified.');
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const approverId = this.auth.currentUser()?.userId;
    if (!approverId) {
      this.toast.error('Not signed in.');
      return;
    }

    const v = this.form.getRawValue();
    const ok = await this.confirm.confirm({
      title: `${v.decisionValue} this application?`,
      message: `This is the final decision on application <strong>#${this.applicationId()}</strong>. It cannot be undone from here.`,
      confirmText: v.decisionValue,
      variant: v.decisionValue === 'Rejected' ? 'danger' : 'primary',
      icon: v.decisionValue === 'Rejected' ? 'bi-x-circle' : 'bi-check2-circle',
    });
    if (!ok) return;

    this.submitting.set(true);
    this.api.decide({
      applicationId: this.applicationId(),
      approverId,
      decisionValue: v.decisionValue,
      notes: v.notes.trim(),
      // Backend DecisionDto requires Date â€” send "now" in ISO format.
      date: new Date().toISOString(),
    }).subscribe({
      next: res => {
        this.submitting.set(false);
        this.toast.success(res.message || 'Decision recorded.');
        this.router.navigate(['/approver/decisions']);
      },
      error: (err: Error) => {
        this.submitting.set(false);
        this.toast.error(err.message);
      },
    });
  }

  // ---------- helpers ----------------------------------------------------

  protected decisionClass(d: 'Approved' | 'Rejected' | 'Pending'): string {
    switch (d) {
      case 'Approved': return 'bg-success-subtle text-success-emphasis';
      case 'Rejected': return 'bg-danger-subtle text-danger-emphasis';
      default:         return 'bg-warning-subtle text-warning-emphasis';
    }
  }
  protected decisionIcon(d: 'Approved' | 'Rejected' | 'Pending'): string {
    switch (d) {
      case 'Approved': return 'bi-check-circle';
      case 'Rejected': return 'bi-x-circle';
      default:         return 'bi-hourglass-split';
    }
  }

  protected initials(name: string, email: string): string {
    const source = (name && name.trim()) || (email?.split('@')[0] ?? '?');
    const parts = source.split(/[\s._-]+/).filter(Boolean);
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
    return source.slice(0, 2).toUpperCase();
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

  protected get decisionValue() { return this.form.get('decisionValue')!; }
  protected get notes() { return this.form.get('notes')!; }
}
