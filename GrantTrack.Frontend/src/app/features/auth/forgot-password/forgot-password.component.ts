import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { AuthLayoutComponent } from '../../../shared/components/auth-layout/auth-layout.component';

/**
 * Self-service reset â€” POST /api/v1/user/forgotpassword.
 *
 * The backend AuthService validates the password against this regex:
 *   ^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$
 * (DTO regex is slightly weaker, the service is the source of truth.)
 */
@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, AuthLayoutComponent],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css',
})
export class ForgotPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);

  private static readonly EMAIL_RE = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
  private static readonly STRONG_PWD_RE =
    /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$/;

  protected readonly form: FormGroup = this.fb.nonNullable.group(
    {
      email: ['', [Validators.required, Validators.pattern(ForgotPasswordComponent.EMAIL_RE)]],
      newPassword: ['', [Validators.required, Validators.pattern(ForgotPasswordComponent.STRONG_PWD_RE)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: [ForgotPasswordComponent.matchPasswords] },
  );

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(null);
  protected showPassword = false;

  submit(): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.auth.forgotPassword(this.form.getRawValue()).subscribe({
      next: msg => {
        this.submitting.set(false);
        this.successMessage.set(msg || 'Password updated successfully.');
        this.form.reset({ email: '', newPassword: '', confirmPassword: '' });
      },
      error: (err: Error) => {
        this.submitting.set(false);
        this.errorMessage.set(err.message);
      },
    });
  }

  private static matchPasswords(group: AbstractControl): ValidationErrors | null {
    const a = group.get('newPassword')?.value;
    const b = group.get('confirmPassword')?.value;
    return a && b && a !== b ? { passwordMismatch: true } : null;
  }

  protected get email() { return this.form.get('email')!; }
  protected get newPassword() { return this.form.get('newPassword')!; }
  protected get confirmPassword() { return this.form.get('confirmPassword')!; }
}
