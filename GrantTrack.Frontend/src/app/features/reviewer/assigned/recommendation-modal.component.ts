import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ReviewDecision } from '../../../core/models/enums';
import { ReviewFilterResponse } from '../../../core/models/review.models';
import { AppDocument } from '../../../core/models/document.models';
import { ReviewerService } from '../services/reviewer.service';
import { DocumentApiService } from '../../../core/services/document-api.service';
import { ToastService } from '../../../shared/services/toast.service';

/**
 * Modal that captures a reviewer's recommendation for a single assigned
 * review. The form mirrors RecommendationRequestDto exactly:
 *   - decision: "Approved" | "Rejected" (Pending is excluded â€” see service)
 *   - score:    integer 1..10  (BE regex /^([1-9]|10)$/)
 *   - notes + comments
 *
 * The modal also pulls the applicant's uploaded documents via
 * GET /api/v1/applications/{id}/documents (allowed because this reviewer
 * is assigned to the application â€” the BE service double-checks).
 *
 * On success, the modal resolves with the saved decision so the host page
 * can patch the row in its list without a full refetch.
 */
@Component({
  selector: 'app-recommendation-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './recommendation-modal.component.html',
  styleUrl: './recommendation-modal.component.css',
})
export class RecommendationModalComponent implements OnInit {
  protected readonly active = inject(NgbActiveModal);
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ReviewerService);
  private readonly docApi = inject(DocumentApiService);
  private readonly toast = inject(ToastService);

  /** The review row being acted on. */
  @Input({ required: true }) review!: ReviewFilterResponse;
  /** Reviewer id to send in the payload â€” owns this review. */
  @Input({ required: true }) reviewerId!: number;

  protected readonly decisions: ReviewDecision[] = ReviewerService.SUBMITTABLE_DECISIONS;
  protected readonly scoreOptions = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
  protected readonly busy = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  // Document panel state
  protected readonly documents = signal<AppDocument[]>([]);
  protected readonly loadingDocs = signal(false);
  protected readonly docLoadError = signal<string | null>(null);
  protected readonly downloadingDocId = signal<number | null>(null);

  protected form!: FormGroup;

  ngOnInit(): void {
    this.form = this.fb.nonNullable.group({
      decision: ['Approved' as ReviewDecision, [Validators.required]],
      score: [8, [Validators.required, Validators.min(1), Validators.max(10)]],
      notes: ['', [Validators.required, Validators.maxLength(2000)]],
      comments: ['', [Validators.maxLength(500)]],
    });

    this.loadDocuments();
  }

  /** Pull the applicant's docs so the reviewer can read them while deciding. */
  loadDocuments(): void {
    this.loadingDocs.set(true);
    this.docLoadError.set(null);
    this.docApi.list(this.review.applicationId).subscribe({
      next: list => {
        this.documents.set(list);
        this.loadingDocs.set(false);
      },
      error: (err: Error) => {
        this.docLoadError.set(err.message);
        this.loadingDocs.set(false);
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

  /** Click handler for the decision pills. */
  setDecision(d: ReviewDecision): void {
    this.form.patchValue({ decision: d });
  }
  /** Click handler for the score chips (1..10). */
  setScore(n: number): void {
    this.form.patchValue({ score: n });
  }

  submit(): void {
    this.errorMessage.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.busy.set(true);

    this.api.submitRecommendation({
      applicationId: this.review.applicationId,
      reviewerId: this.reviewerId,
      decision: v.decision,
      notes: v.notes.trim(),
      score: Number(v.score),
      comments: v.comments.trim(),
    }).subscribe({
      next: res => this.active.close({
        decision: v.decision as ReviewDecision,
        message: res?.message ?? 'Recommendation submitted.',
      }),
      error: (err: Error) => {
        this.busy.set(false);
        this.errorMessage.set(err.message);
      },
    });
  }

  // Helpers --------------------------------------------------------------

  protected fileIcon(name: string): string {
    const ext = name.split('.').pop()?.toLowerCase() ?? '';
    if (['pdf'].includes(ext)) return 'bi-file-earmark-pdf';
    if (['jpg', 'jpeg', 'png', 'gif'].includes(ext)) return 'bi-file-earmark-image';
    if (['doc', 'docx'].includes(ext)) return 'bi-file-earmark-word';
    if (['xls', 'xlsx'].includes(ext)) return 'bi-file-earmark-excel';
    if (['txt'].includes(ext)) return 'bi-file-earmark-text';
    return 'bi-file-earmark';
  }

  // Template accessor helpers
  protected get decision() { return this.form.get('decision')!; }
  protected get score() { return this.form.get('score')!; }
  protected get notes() { return this.form.get('notes')!; }
  protected get comments() { return this.form.get('comments')!; }
}
