import { Component, Input, inject } from '@angular/core';
import { NgbActiveModal, NgbModal } from '@ng-bootstrap/ng-bootstrap';

export interface ConfirmDialogOptions {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  /** 'danger' colours the confirm button red; 'primary' is neutral. */
  variant?: 'danger' | 'primary';
  icon?: string;       // bootstrap-icons class, e.g. 'bi-trash'
}

/**
 * Generic Yes/No confirmation modal. Use `ConfirmDialogService.confirm({...})`
 * to open it — it returns a Promise<boolean> resolving to true on confirm.
 */
@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  template: `
    <div class="modal-body p-4 text-center">
      <div class="confirm-icon mx-auto mb-3" [class]="iconWrapClass">
        <i class="bi" [class]="icon || (variant === 'danger' ? 'bi-exclamation-triangle' : 'bi-question-circle')"></i>
      </div>
      <h2 class="h5 fw-semibold mb-2">{{ title }}</h2>
      <p class="text-body-secondary mb-0" [innerHTML]="message"></p>
    </div>
    <div class="modal-footer border-0 justify-content-center pb-4">
      <button type="button" class="btn btn-outline-secondary px-4"
              (click)="active.dismiss(false)">{{ cancelText }}</button>
      <button type="button" class="btn px-4"
              [class.btn-danger]="variant === 'danger'"
              [class.btn-primary]="variant !== 'danger'"
              (click)="active.close(true)">{{ confirmText }}</button>
    </div>
  `,
  styles: [`
    .confirm-icon {
      width: 64px; height: 64px; border-radius: 16px;
      display: inline-flex; align-items: center; justify-content: center;
      font-size: 1.75rem;
    }
    .confirm-icon--danger  { background: rgba(220,38,38,.10); color: #dc2626; }
    .confirm-icon--primary { background: rgba(30,58,138,.10); color: #1e3a8a; }
  `],
})
export class ConfirmDialogComponent implements ConfirmDialogOptions {
  protected readonly active = inject(NgbActiveModal);

  @Input() title = 'Are you sure?';
  @Input() message = '';
  @Input() confirmText = 'Confirm';
  @Input() cancelText = 'Cancel';
  @Input() variant: 'danger' | 'primary' = 'primary';
  @Input() icon?: string;

  protected get iconWrapClass(): string {
    return this.variant === 'danger' ? 'confirm-icon--danger' : 'confirm-icon--primary';
  }
}

/**
 * Convenience service so callers don't have to import NgbModal directly.
 */
import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
  private readonly modal = inject(NgbModal);

  confirm(opts: ConfirmDialogOptions): Promise<boolean> {
    const ref = this.modal.open(ConfirmDialogComponent, {
      centered: true,
      backdrop: 'static',
      size: 'sm',
      windowClass: 'gt-confirm-modal',
    });
    Object.assign(ref.componentInstance, opts);
    return ref.result.then(
      (v: boolean) => v === true,
      () => false,
    );
  }
}
