import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbToastModule } from '@ng-bootstrap/ng-bootstrap';
import { ToastService } from '../../services/toast.service';

/**
 * Renders the toast queue at a fixed position on screen. Drop one
 * <app-toast-host /> in the root layout so feature pages don't need to
 * worry about positioning.
 */
@Component({
  selector: 'app-toast-host',
  standalone: true,
  imports: [CommonModule, NgbToastModule],
  template: `
    <div class="toast-host" aria-live="polite" aria-atomic="true">
      @for (t of toasts.toasts(); track t.id) {
        <ngb-toast
          [class]="'toast-item border-0 shadow-sm bg-' + t.variant"
          [autohide]="false"
          (hidden)="toasts.dismiss(t.id)">
          <div class="d-flex align-items-start gap-2 text-white">
            <i class="bi" [class]="iconFor(t.variant)"></i>
            <span class="flex-grow-1">{{ t.message }}</span>
            <button type="button" class="btn-close btn-close-white btn-sm"
                    (click)="toasts.dismiss(t.id)" aria-label="Close"></button>
          </div>
        </ngb-toast>
      }
    </div>
  `,
  styles: [`
    .toast-host {
      position: fixed;
      top: 80px;
      right: 1rem;
      z-index: 1080;
      display: flex;
      flex-direction: column;
      gap: .5rem;
      max-width: min(90vw, 380px);
    }
    .toast-item {
      border-radius: .625rem;
      padding: .75rem .9rem;
      font-size: .9rem;
      backdrop-filter: blur(4px);
    }
    .bg-success { background: #16a34a !important; }
    .bg-danger  { background: #dc2626 !important; }
    .bg-warning { background: #f59e0b !important; }
    .bg-info    { background: #0ea5e9 !important; }
  `],
})
export class ToastHostComponent {
  protected readonly toasts = inject(ToastService);

  protected iconFor(variant: string): string {
    switch (variant) {
      case 'success': return 'bi-check-circle-fill';
      case 'danger':  return 'bi-exclamation-triangle-fill';
      case 'warning': return 'bi-exclamation-circle-fill';
      default:        return 'bi-info-circle-fill';
    }
  }
}
