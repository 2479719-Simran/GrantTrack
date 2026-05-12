import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Brand mark used in the navbar and on the auth pages.
 * Inline SVG so it inherits currentColor and doesn't add a network request.
 *
 * Usage:
 *   <app-brand-logo size="md" />          – just the mark
 *   <app-brand-logo [withWordmark]="true" /> – mark + "GrantTrack" text
 */
@Component({
  selector: 'app-brand-logo',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span class="brand d-inline-flex align-items-center" [class.brand--light]="light">
      <svg
        class="brand__mark"
        [attr.width]="markPx"
        [attr.height]="markPx"
        viewBox="0 0 32 32"
        xmlns="http://www.w3.org/2000/svg"
        aria-hidden="true">
        <!-- Rounded square plate -->
        <rect width="32" height="32" rx="8" [attr.fill]="light ? '#ffffff' : '#0f1e4a'"/>
        <!-- Stylised G mark + accent dot -->
        <path d="M21.5 11.5a6.2 6.2 0 1 0 1.5 8.6h-5.4v-2.4h7.5v.5a8.6 8.6 0 1 1-2.5-6l-1.1 -.7Z"
              [attr.fill]="light ? '#0f1e4a' : '#ffffff'"/>
        <circle cx="24" cy="11" r="2.4" fill="#0ea5e9"/>
      </svg>
      @if (withWordmark) {
        <span class="brand__wordmark ms-2">GrantTrack</span>
      }
    </span>
  `,
  styles: [`
    .brand { line-height: 1; }
    .brand__wordmark {
      font-weight: 700;
      letter-spacing: -0.02em;
      font-size: 1.05rem;
      color: inherit;
    }
    .brand--light .brand__wordmark { color: #ffffff; }
  `],
})
export class BrandLogoComponent {
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  /** Show the "GrantTrack" wordmark next to the symbol. */
  @Input() withWordmark = false;
  /** Light variant for use on dark backgrounds (navbar). */
  @Input() light = false;

  protected get markPx(): number {
    switch (this.size) {
      case 'sm': return 22;
      case 'lg': return 40;
      default:   return 28;
    }
  }
}
