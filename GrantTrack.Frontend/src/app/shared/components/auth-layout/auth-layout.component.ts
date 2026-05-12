import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BrandLogoComponent } from '../brand-logo/brand-logo.component';

/**
 * Reusable two-pane shell for the auth screens (login / register / forgot).
 * - Left pane: brand panel with hero copy + product highlights.
 *   Hidden on small screens â€” only shows from md (â‰¥768px) up.
 * - Right pane: <ng-content> from the host page (the actual form card).
 *
 * Each consumer passes a small `headline` / `lede` pair via inputs to give
 * the page a unique tone without duplicating the layout markup.
 */
@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [CommonModule, BrandLogoComponent],
  templateUrl: './auth-layout.component.html',
  styleUrl: './auth-layout.component.css',
})
export class AuthLayoutComponent {
  @Input() headline = 'Manage the full grant lifecycle.';
  @Input() lede =
    'GrantTrack brings applications, reviews, decisions, disbursements and compliance into one trusted workflow.';

  protected readonly year = new Date().getFullYear();
}
