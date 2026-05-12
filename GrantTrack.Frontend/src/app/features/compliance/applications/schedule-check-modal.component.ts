import { Component, Input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ComplianceType } from '../../../core/models/enums';

/**
 * Schedules a new compliance check on an approved application.
 * Resolution: { applicationId, type, notes } via active.close â€” the host
 * page calls the API + refreshes.
 */
@Component({
  selector: 'app-schedule-check-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './schedule-check-modal.component.html',
  styleUrl: './schedule-check-modal.component.css',
})
export class ScheduleCheckModalComponent implements OnInit {
  protected readonly active = inject(NgbActiveModal);
  private readonly fb = inject(FormBuilder);

  @Input({ required: true }) applicationId!: number;
  @Input() applicantName: string = '';
  /** Types the host has already scheduled â€” disable them in the picker so
   *  the CO doesn't accidentally double-schedule the same type. */
  @Input() existingTypes: ComplianceType[] = [];

  protected readonly types: ComplianceType[] = ['Financial', 'Operational'];

  protected form!: FormGroup;

  ngOnInit(): void {
    // Default to whichever type isn't already scheduled (if any).
    const firstAvailable = this.types.find(t => !this.existingTypes.includes(t)) ?? 'Financial';
    this.form = this.fb.nonNullable.group({
      type: [firstAvailable as ComplianceType, [Validators.required]],
      notes: ['', [Validators.required, Validators.maxLength(2000)]],
    });
  }

  setType(t: ComplianceType): void {
    if (this.existingTypes.includes(t)) return;
    this.form.patchValue({ type: t });
  }

  isExisting(t: ComplianceType): boolean { return this.existingTypes.includes(t); }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.active.close({
      applicationId: this.applicationId,
      type: v.type,
      notes: v.notes.trim(),
    });
  }

  protected get type() { return this.form.get('type')!; }
  protected get notes() { return this.form.get('notes')!; }
}
