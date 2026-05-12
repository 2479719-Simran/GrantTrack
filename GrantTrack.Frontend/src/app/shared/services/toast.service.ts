import { Injectable, signal } from '@angular/core';

export type ToastVariant = 'success' | 'danger' | 'info' | 'warning';

export interface Toast {
  id: number;
  message: string;
  variant: ToastVariant;
  /** Auto-hide delay in milliseconds. Set to 0 to keep until dismissed. */
  delay: number;
}

/**
 * Lightweight toast queue used app-wide for transient feedback (success
 * confirmations, validation errors, etc.). The host <app-toast-host>
 * subscribes to `toasts` and renders the stack.
 *
 * Why a signal: avoids ChangeDetectorRef ceremony in the host and stays
 * decoupled from RxJS — toasts are simple ephemeral state.
 */
@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 1;
  readonly toasts = signal<Toast[]>([]);

  show(message: string, variant: ToastVariant = 'info', delay = 4000): void {
    const toast: Toast = { id: this.nextId++, message, variant, delay };
    this.toasts.update(list => [...list, toast]);
    if (delay > 0) {
      setTimeout(() => this.dismiss(toast.id), delay);
    }
  }

  success(message: string, delay = 3500): void { this.show(message, 'success', delay); }
  error(message: string, delay = 6000): void { this.show(message, 'danger', delay); }
  info(message: string, delay = 4000): void { this.show(message, 'info', delay); }
  warning(message: string, delay = 4500): void { this.show(message, 'warning', delay); }

  dismiss(id: number): void {
    this.toasts.update(list => list.filter(t => t.id !== id));
  }

  clear(): void {
    this.toasts.set([]);
  }
}
