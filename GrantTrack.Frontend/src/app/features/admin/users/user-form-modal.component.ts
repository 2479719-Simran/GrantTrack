import { Component, Input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ALL_ROLES, UserRole } from '../../../core/models/enums';
import { ViewUser } from '../../../core/models/user.models';

/**
 * Reused for both creating and editing a user. Same field set, slightly
 * different validation rules (password is only collected on create).
 *
 * Resolution model:
 *   close({ name, email, phone, role, status, password? })  â†’ caller saves
 *   dismiss()                                                â†’ caller cancels
 *
 * The host page is responsible for calling the right service (create vs
 * update) and surfacing toasts.
 */
@Component({
  selector: 'app-user-form-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-form-modal.component.html',
  styleUrl: './user-form-modal.component.css',
})
export class UserFormModalComponent implements OnInit {
  protected readonly active = inject(NgbActiveModal);
  private readonly fb = inject(FormBuilder);

  /** When set, the modal is in EDIT mode; otherwise CREATE mode. */
  @Input() user: ViewUser | null = null;

  protected readonly roles: UserRole[] = ALL_ROLES;
  protected readonly emailRe = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
  protected readonly phoneRe = /^\d{10}$/;
  protected readonly strongPwdRe =
    /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$/;

  protected form!: FormGroup;
  protected showPassword = false;
  protected submitError: string | null = null;

  get isEdit(): boolean { return this.user !== null; }
  get title(): string { return this.isEdit ? 'Edit user' : 'Add user'; }
  get submitLabel(): string { return this.isEdit ? 'Save changes' : 'Create user'; }

  ngOnInit(): void {
    this.form = this.fb.nonNullable.group(
      {
        name: [this.user?.name ?? '', [Validators.required, Validators.maxLength(120)]],
        email: [this.user?.email ?? '', [Validators.required, Validators.pattern(this.emailRe)]],
        // Phone is required in BOTH modes â€” UserService.UpdateUser rejects
        // null/non-10-digit even when other fields haven't changed. ViewUser
        // doesn't carry the existing phone, so the admin re-enters it.
        phone: ['', [Validators.required, Validators.pattern(this.phoneRe)]],
        role: [(this.user?.role as UserRole) ?? 'Applicant'],
        status: [this.user?.status ?? true],
        password: [''],
        confirmPassword: [''],
      },
      { validators: [this.matchPasswords] },
    );

    // Create mode â†’ password becomes required + must satisfy the strong rule.
    if (!this.isEdit) {
      this.form.get('password')!.addValidators([
        Validators.required,
        Validators.pattern(this.strongPwdRe),
      ]);
      this.form.get('confirmPassword')!.addValidators([Validators.required]);
    }
  }

  submit(): void {
    this.submitError = null;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    // Only include password in payload when creating.
    const payload = this.isEdit
      ? { name: v.name, email: v.email, phone: v.phone, role: v.role, status: v.status }
      : {
          name: v.name, email: v.email, phone: v.phone, role: v.role,
          status: v.status, password: v.password,
        };
    this.active.close(payload);
  }

  // Cross-field validator for the password+confirm pair.
  private matchPasswords = (group: AbstractControl): ValidationErrors | null => {
    if (this.isEdit) return null;          // not used in edit mode
    const a = group.get('password')?.value;
    const b = group.get('confirmPassword')?.value;
    return a && b && a !== b ? { passwordMismatch: true } : null;
  };

  protected get name() { return this.form.get('name')!; }
  protected get email() { return this.form.get('email')!; }
  protected get phone() { return this.form.get('phone')!; }
  protected get role() { return this.form.get('role')!; }
  protected get status() { return this.form.get('status')!; }
  protected get password() { return this.form.get('password')!; }
  protected get confirmPassword() { return this.form.get('confirmPassword')!; }
}
