import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Consistent header for top of every feature page.
 *
 * Usage:
 *   <app-page-header title="Users" subtitle="Manage roles and access">
 *     <button class="btn btn-primary"><i class="bi bi-plus"></i> Add user</button>
 *   </app-page-header>
 *
 * The action buttons go in the projected slot.
 */
@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [CommonModule],
  template: `
    <header class="page-header d-flex flex-wrap align-items-end justify-content-between gap-3">
      <div>
        @if (eyebrow) {
          <p class="text-uppercase fw-semibold small text-body-secondary mb-1 letter-spaced">
            {{ eyebrow }}
          </p>
        }
        <h1 class="h3 fw-bold mb-1 d-flex align-items-center gap-2">
          @if (icon) { <i class="bi" [class]="icon"></i> }
          {{ title }}
        </h1>
        @if (subtitle) {
          <p class="text-body-secondary mb-0">{{ subtitle }}</p>
        }
      </div>
      <div class="page-header__actions d-flex flex-wrap gap-2">
        <ng-content></ng-content>
      </div>
    </header>
    <hr class="page-header__divider" />
  `,
  styles: [`
    .page-header { padding-top: 1.25rem; }
    .page-header__divider {
      margin: 1.25rem 0 1.5rem;
      border: 0;
      border-top: 1px solid var(--gt-border);
    }
    :host { display: block; }
    .letter-spaced { letter-spacing: .08em; }
    h1 .bi { color: var(--gt-navy-700); font-size: 1.4rem; }
  `],
})
export class PageHeaderComponent {
  @Input({ required: true }) title!: string;
  @Input() subtitle?: string;
  @Input() eyebrow?: string;
  @Input() icon?: string;     // e.g. 'bi-people'
}
