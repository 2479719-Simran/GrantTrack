import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { AuthLayoutComponent } from '../../../shared/components/auth-layout/auth-layout.component';

/**
 * Sign-in form â€” POST /api/v1/user/login.
 *
 * Validation matches the API: Email regex from the User entity and a
 * non-empty password (the backend enforces stronger rules at registration
 * but does not re-validate password format on login).
 */
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, AuthLayoutComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  /** Mirrors the backend regex on User.Email (also the RegisterUserDto regex). */
  private static readonly EMAIL_RE = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;

  protected readonly form: FormGroup = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.pattern(LoginComponent.EMAIL_RE)]],
    password: ['', [Validators.required]],
  });

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected showPassword = false;

  submit(): void {
    this.errorMessage.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.submitting.set(false);
        // Honour returnUrl if present; otherwise drop user on the home page.
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        this.router.navigateByUrl(returnUrl ?? '/home');
      },
      error: (err: Error) => {
        this.submitting.set(false);
        this.errorMessage.set(err.message);
      },
    });
  }

  // Accessor helpers keep the template tidy.
  protected get email() { return this.form.get('email')!; }
  protected get password() { return this.form.get('password')!; }
}
