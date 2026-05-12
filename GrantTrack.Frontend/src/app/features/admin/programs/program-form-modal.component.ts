import { Component, Input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { GetProgram } from '../../../core/models/program.models';

/**
 * Reused for create + edit. Resolves to the request payload (sans
 * programId) on save, or dismisses on cancel.
 */
@Component({
  selector: 'app-program-form-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './program-form-modal.component.html',
  styleUrl: './program-form-modal.component.css',
})
export class ProgramFormModalComponent implements OnInit {
  protected readonly active = inject(NgbActiveModal);
  private readonly fb = inject(FormBuilder);

  @Input() program: GetProgram | null = null;

  protected form!: FormGroup;

  get isEdit(): boolean { return this.program !== null; }
  get title(): string { return this.isEdit ? 'Edit program' : 'Create program'; }
  get submitLabel(): string { return this.isEdit ? 'Save changes' : 'Create program'; }

  ngOnInit(): void {
    const today = new Date();
    const isoToday = today.toISOString().substring(0, 10);
    const isoNextYear = new Date(today.getFullYear() + 1, today.getMonth(), today.getDate())
      .toISOString().substring(0, 10);

    this.form = this.fb.nonNullable.group(
      {
        name: [this.program?.name ?? '', [Validators.required, Validators.maxLength(160)]],
        description: [this.program?.description ?? '', [Validators.required, Validators.maxLength(2000)]],
        startDate: [this.toDateInputValue(this.program?.startDate) ?? isoToday, [Validators.required]],
        endDate: [this.toDateInputValue(this.program?.endDate) ?? isoNextYear, [Validators.required]],
        budget: [this.program?.budget ?? 0, [Validators.required, Validators.min(0.01)]],
        status: [this.program?.status ?? true],
      },
      { validators: [this.endAfterStart] },
    );
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.active.close({
      name: v.name.trim(),
      description: v.description.trim(),
      // The backend expects ISO date strings; the <input type="date">
      // already gives us yyyy-mm-dd which System.Text.Json parses cleanly.
      startDate: v.startDate,
      endDate: v.endDate,
      budget: Number(v.budget),
      status: !!v.status,
    });
  }

  /** Convert backend ISO string to yyyy-mm-dd for the <input type="date">. */
  private toDateInputValue(iso: string | undefined): string | null {
    if (!iso) return null;
    return iso.length >= 10 ? iso.substring(0, 10) : iso;
  }

  // Cross-field validator: endDate must be on/after startDate.
  private endAfterStart = (group: any) => {
    const s = group.get('startDate')?.value;
    const e = group.get('endDate')?.value;
    if (!s || !e) return null;
    return new Date(e) < new Date(s) ? { dateOrder: true } : null;
  };

  protected get name() { return this.form.get('name')!; }
  protected get description() { return this.form.get('description')!; }
  protected get startDate() { return this.form.get('startDate')!; }
  protected get endDate() { return this.form.get('endDate')!; }
  protected get budget() { return this.form.get('budget')!; }
  protected get status() { return this.form.get('status')!; }
}
