import { Component, Input, OnInit, inject } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { FinanceDisbursementInfo } from '../../../core/models/finance.models';
import { PaymentMethod } from '../../../core/models/enums';

/**
 * Records a payment against a Scheduled disbursement tranche.
 *
 * BE rules respected here:
 *   â€¢ Tranche must be in Scheduled status (host page disables the open
 *     button otherwise).
 *   â€¢ Amount cannot exceed the tranche's remaining headroom â€” we enforce
 *     this client-side too so the user gets immediate feedback.
 */
@Component({
  selector: 'app-payment-form-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DecimalPipe],
  templateUrl: './payment-form-modal.component.html',
  styleUrl: './payment-form-modal.component.css',
})
export class PaymentFormModalComponent implements OnInit {
  protected readonly active = inject(NgbActiveModal);
  private readonly fb = inject(FormBuilder);

  @Input({ required: true }) disbursement!: FinanceDisbursementInfo;

  protected readonly methods: PaymentMethod[] = ['BankTransfer', 'Cheque', 'Cash', 'OnlineTransfer'];

  protected form!: FormGroup;

  ngOnInit(): void {
    const todayIso = new Date().toISOString().substring(0, 10);
    this.form = this.fb.nonNullable.group({
      amount: [
        this.disbursement.remainingOnTranche,
        [
          Validators.required,
          Validators.min(0.01),
          // Cap at the tranche's remaining headroom â€” caller couldn't open
          // the modal if remaining was 0, so this is just an upper-bound.
          Validators.max(this.disbursement.remainingOnTranche),
        ],
      ],
      date: [todayIso, [Validators.required]],
      method: ['BankTransfer' as PaymentMethod, [Validators.required]],
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.active.close({
      disbursementId: this.disbursement.disbursementId,
      amount: Number(v.amount),
      date: v.date,
      method: v.method,
    });
  }

  protected get amount() { return this.form.get('amount')!; }
  protected get date() { return this.form.get('date')!; }
  protected get method() { return this.form.get('method')!; }

  /** Friendly labels for the payment-method dropdown. */
  protected methodLabel(m: PaymentMethod): string {
    return m.replace(/([a-z])([A-Z])/g, '$1 $2');
  }
}
