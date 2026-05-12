import { Component, Input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ComplianceCheckInfo } from '../../../core/models/compliance.models';
import { ComplianceResult } from '../../../core/models/enums';

/**
 * Finalises a compliance check â€” sets the result (Completed = verified
 * the grantee's report; Flagged = issues found, report returned for fixes)
 * and final notes.
 *
 * Backend caveat: ComplianceCheckService.CompleteCheckAsync requires a
 * GrantReport row to exist for the application. The BE has no endpoint
 * to create one yet, so the user will see "No Grant Report found to
 * verify" until that's added. We surface this as a clean toast at the
 * host page, and warn the user up-front in this modal.
 */
@Component({
  selector: 'app-complete-check-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './complete-check-modal.component.html',
  styleUrl: './complete-check-modal.component.css',
})
export class CompleteCheckModalComponent implements OnInit {
  protected readonly active = inject(NgbActiveModal);
  private readonly fb = inject(FormBuilder);

  @Input({ required: true }) check!: ComplianceCheckInfo;
  /** Whether the application has a GrantReport â€” the BE blocks completion
   *  if not. We pre-warn the user. */
  @Input() hasGrantReport = false;

  protected readonly results: ComplianceResult[] = ['Completed', 'Flagged'];
  protected form!: FormGroup;

  ngOnInit(): void {
    this.form = this.fb.nonNullable.group({
      // Default to Completed â€” the typical happy-path outcome.
      result: ['Completed' as ComplianceResult, [Validators.required]],
      notes: [this.check.notes ?? '', [Validators.required, Validators.maxLength(2000)]],
    });
  }

  setResult(r: ComplianceResult): void {
    this.form.patchValue({ result: r });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.active.close({
      checkId: this.check.checkId,
      result: v.result,
      notes: v.notes.trim(),
    });
  }

  protected get result() { return this.form.get('result')!; }
  protected get notes() { return this.form.get('notes')!; }
}
