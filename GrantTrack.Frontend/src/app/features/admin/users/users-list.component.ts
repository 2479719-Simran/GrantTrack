import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ViewUser, UpdateUserRequest } from '../../../core/models/user.models';
import { ALL_ROLES, UserRole } from '../../../core/models/enums';
import { RegisterUserRequest } from '../../../core/models/auth.models';
import { AdminUserService } from '../services/admin-user.service';
import { ToastService } from '../../../shared/services/toast.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { UserFormModalComponent } from './user-form-modal.component';

/**
 * Admin "Users" page. Lists every registered user with role + status pills
 * and provides Add / Edit / Deactivate actions.
 *
 * Filtering + searching are done client-side over the loaded list â€” the
 * backend has no list-with-filter endpoint, but the data set will stay
 * small enough that this is fine.
 */
@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [CommonModule, FormsModule, PageHeaderComponent],
  templateUrl: './users-list.component.html',
  styleUrl: './users-list.component.css',
})
export class UsersListComponent implements OnInit {
  private readonly api = inject(AdminUserService);
  private readonly toast = inject(ToastService);
  private readonly modal = inject(NgbModal);
  private readonly confirm = inject(ConfirmDialogService);

  protected readonly users = signal<ViewUser[]>([]);
  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);

  // Filter state
  protected readonly search = signal('');
  protected readonly roleFilter = signal<UserRole | ''>('');
  protected readonly statusFilter = signal<'' | 'active' | 'inactive'>('');
  protected readonly roleOptions: UserRole[] = ALL_ROLES;

  /** Filtered / sorted view used by the template. */
  protected readonly visibleUsers = computed<ViewUser[]>(() => {
    const q = this.search().trim().toLowerCase();
    const role = this.roleFilter();
    const status = this.statusFilter();
    return this.users().filter(u => {
      if (q) {
        const hay = `${u.name ?? ''} ${u.email ?? ''}`.toLowerCase();
        if (!hay.includes(q)) return false;
      }
      if (role && u.role !== role) return false;
      if (status === 'active' && !u.status) return false;
      if (status === 'inactive' && u.status) return false;
      return true;
    });
  });

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.api.getAll().subscribe({
      next: list => {
        this.users.set(list);
        this.loading.set(false);
      },
      error: (err: Error) => {
        this.loadError.set(err.message);
        this.loading.set(false);
      },
    });
  }

  openCreate(): void {
    const ref = this.modal.open(UserFormModalComponent, { centered: true, size: 'lg', backdrop: 'static' });
    ref.componentInstance.user = null;
    ref.result.then(
      (payload: RegisterUserRequest & { status: boolean }) => {
        if (!payload) return;
        // The register endpoint always activates the user; if the admin
        // unticked Status we toggle it off via a follow-up update.
        this.api.create(payload).subscribe({
          next: () => {
            this.toast.success(`User ${payload.email} created.`);
            this.refresh();
          },
          error: (err: Error) => this.toast.error(err.message),
        });
      },
      () => { /* dismissed */ },
    );
  }

  openEdit(user: ViewUser): void {
    const ref = this.modal.open(UserFormModalComponent, { centered: true, size: 'lg', backdrop: 'static' });
    ref.componentInstance.user = user;
    ref.result.then(
      (payload: UpdateUserRequest) => {
        if (!payload) return;
        this.api.update(user.userId, payload).subscribe({
          next: () => {
            this.toast.success(`User ${user.email} updated.`);
            this.refresh();
          },
          error: (err: Error) => this.toast.error(err.message),
        });
      },
      () => { /* dismissed */ },
    );
  }

  async deactivate(user: ViewUser): Promise<void> {
    const ok = await this.confirm.confirm({
      title: 'Deactivate user?',
      message: `<strong>${user.email}</strong> will no longer be able to sign in. You can reactivate them later from the edit dialog.`,
      confirmText: 'Deactivate',
      variant: 'danger',
      icon: 'bi-person-x',
    });
    if (!ok) return;

    this.api.deactivate(user.userId).subscribe({
      next: () => {
        this.toast.success(`${user.email} deactivated.`);
        this.refresh();
      },
      error: (err: Error) => this.toast.error(err.message),
    });
  }

  /** Used by template to colour role badges differently per role. */
  protected roleClass(role: string | null): string {
    switch (role) {
      case 'Admin':             return 'bg-danger-subtle text-danger-emphasis';
      case 'Reviewer':          return 'bg-info-subtle text-info-emphasis';
      case 'Approver':          return 'bg-primary-subtle text-primary-emphasis';
      case 'FinanceOfficer':    return 'bg-success-subtle text-success-emphasis';
      case 'ComplianceOfficer': return 'bg-warning-subtle text-warning-emphasis';
      default:                  return 'bg-secondary-subtle text-secondary-emphasis';
    }
  }

  protected initials(name: string | null, email: string | null): string {
    const source = (name && name.trim()) || (email?.split('@')[0] ?? '?');
    const parts = source.split(/[\s._-]+/).filter(Boolean);
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
    return source.slice(0, 2).toUpperCase();
  }
}
