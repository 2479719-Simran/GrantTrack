import { Component, Input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { GrantReport } from '../../../core/models/grant-report.models';

/**
 * Submit-or-update modal for an applicant's Grant Report. The BE treats
 * POST /api/v1/grantreport as an upsert keyed by applicationId, so this
 * single dialog handles both "first submission" and "re-submission after
 * a Returned check".
 *
 * Resolution shape: { applicationId, scope, metrics, notes?, evidenceDocumentPath? }
 */
@Component({
  selector: 'app-grant-report-form-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './grant-report-form-modal.component.html',
  styleUrl: './grant-report-form-modal.component.css',
})
export class GrantReportFormModalComponent implements OnInit {
  protected readonly active = inject(NgbActiveModal);
  private readonly fb = inject(FormBuilder);

  @Input({ required: true }) applicationId!: number;
  @Input() programName: string = '';
  /** Existing report â€” when present, the form opens pre-filled in update mode. */
  @Input() existing: GrantReport | null = null;

  protected form!: FormGroup;

  get isUpdate(): boolean { return this.existing !== null; }
  get isReturned(): boolean { return this.existing?.status === 'Returned'; }

  get title(): string {
    if (this.isReturned) return 'Resubmit grant report';
    return this.isUpdate ? 'Update grant report' : 'Submit grant report';
  }
  get submitLabel(): string {
    if (this.isReturned) return 'Resubmit for review';
    return this.isUpdate ? 'Save changes' : 'Submit report';
  }

  ngOnInit(): void {
    this.form = this.fb.nonNullable.group({
      scope: [
        this.existing?.scope ?? '',
        [Validators.required, Validators.maxLength(200)],
      ],
      metrics: [
        this.existing?.metrics ?? '',
        [Validators.required, Validators.maxLength(4000)],
      ],
      notes: [this.existing?.notes ?? '', [Validators.maxLength(4000)]],
      evidenceDocumentPath: [
        this.existing?.evidenceDocumentPath ?? '',
        [Validators.maxLength(500)],
      ],
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.active.close({
      applicationId: this.applicationId,
      scope: v.scope.trim(),
      metrics: v.metrics.trim(),
      notes: v.notes?.trim() || null,
      evidenceDocumentPath: v.evidenceDocumentPath?.trim() || null,
    });
  }

  protected get scope() { return this.form.get('scope')!; }
  protected get metrics() { return this.form.get('metrics')!; }
  protected get notes() { return this.form.get('notes')!; }
  protected get evidence() { return this.form.get('evidenceDocumentPath')!; }
}
