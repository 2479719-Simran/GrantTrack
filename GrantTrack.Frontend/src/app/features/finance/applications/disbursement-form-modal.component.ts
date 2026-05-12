import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { FinanceDisbursementInfo } from '../../../core/models/finance.models';
import { DisbursementStatus } from '../../../core/models/enums';

/**
 * Reused for create + update of a disbursement tranche. The dialog flips
 * between modes based on whether `disbursement` was provided.
 *
 *   â€¢ CREATE mode: amount, scheduledDate. New tranche starts as Pending
 *     (BE assigns this automatically).
 *   â€¢ UPDATE mode: amount, scheduledDate, actualDate, status. The status
 *     dropdown shows only legal next states given the current one â€” the
 *     same transition rules the BE enforces (see DisbursementService.cs:
 *     ValidateStatusTransition).
 *
 * Resolution model:
 *   close({ ... payload ... })  â†’  caller hits create() or update()
 *   dismiss()                    â†’  cancel
 */
@Component({
  selector: 'app-disbursement-form-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DecimalPipe],
  templateUrl: './disbursement-form-modal.component.html',
  styleUrl: './disbursement-form-modal.component.css',
})
export class DisbursementFormModalComponent implements OnInit {
  protected readonly active = inject(NgbActiveModal);
  private readonly fb = inject(FormBuilder);

  /** Existing disbursement â†’ update mode; null â†’ create mode. */
  @Input() disbursement: FinanceDisbursementInfo | null = null;
  /** Headroom remaining on the program budget â€” caller computes this. */
  @Input({ required: true }) remainingBudget!: number;

  protected form!: FormGroup;

  /** Status options the user is allowed to set, given the current status.
   *  Mirrors the BE's allowed transitions. */
  protected readonly statusOptions = signal<DisbursementStatus[]>([]);

  get isEdit(): boolean { return this.disbursement !== null; }
  get title(): string { return this.isEdit ? 'Update disbursement' : 'Create disbursement tranche'; }
  get submitLabel(): string { return this.isEdit ? 'Save changes' : 'Create tranche'; }

  ngOnInit(): void {
    const todayIso = new Date().toISOString().substring(0, 10);

    this.form = this.fb.nonNullable.group({
      amount: [
        this.disbursement?.amount ?? 0,
        [Validators.required, Validators.min(0.01)],
      ],
      scheduledDate: [
        this.toDateInput(this.disbursement?.scheduledDate) ?? todayIso,
        [Validators.required],
      ],
      // Update-only fields below.
      actualDate: [this.toDateInput(this.disbursement?.actualDate ?? null) ?? ''],
      status: [(this.disbursement?.status ?? 'Pending') as DisbursementStatus],
    });

    if (this.isEdit) {
      const current = (this.disbursement!.status as DisbursementStatus);
      this.statusOptions.set(this.allowedTransitionsFrom(current));
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();

    if (this.isEdit) {
      // Update mode â€” only send fields that actually changed, so the BE
      // doesn't apply NULLs to fields the user didn't touch.
      const original = this.disbursement!;
      const changes: Record<string, unknown> = {};
      if (Number(v.amount) !== original.amount) changes['amount'] = Number(v.amount);
      if (v.scheduledDate !== this.toDateInput(original.scheduledDate)) {
        changes['scheduledDate'] = v.scheduledDate;
      }
      const newActual = v.actualDate || null;
      if (newActual !== this.toDateInput(original.actualDate)) {
        changes['actualDate'] = newActual;
      }
      if (v.status !== original.status) changes['status'] = v.status;
      this.active.close(changes);
      return;
    }

    // Create mode.
    this.active.close({
      amount: Number(v.amount),
      scheduledDate: v.scheduledDate,
    });
  }

  protected get amount() { return this.form.get('amount')!; }
  protected get scheduledDate() { return this.form.get('scheduledDate')!; }
  protected get actualDate() { return this.form.get('actualDate')!; }
  protected get status() { return this.form.get('status')!; }

  /**
   * Maps current â†’ next-allowed statuses, mirroring the BE's
   * ValidateStatusTransition rules. We always include the current status
   * itself so the form's value stays valid before the user changes it.
   */
  private allowedTransitionsFrom(current: DisbursementStatus): DisbursementStatus[] {
    switch (current) {
      case 'Pending':
        return ['Pending', 'Scheduled', 'Cancelled'];
      case 'Scheduled':
        return ['Scheduled', 'Paid', 'PartiallyPaid', 'Cancelled'];
      // Paid + Cancelled are terminal â€” caller should disable the modal.
      default:
        return [current];
    }
  }

  private toDateInput(iso: string | undefined | null): string | null {
    if (!iso) return null;
    return iso.length >= 10 ? iso.substring(0, 10) : iso;
  }
}
