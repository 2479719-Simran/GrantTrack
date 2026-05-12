import { Component, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { NgbCollapseModule, NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { AuthService } from '../../../core/services/auth.service';
import { BrandLogoComponent } from '../brand-logo/brand-logo.component';

/**
 * Top-of-page navigation. Shown only when the user is logged in. Links are
 * filtered by role so each persona sees only what they can act on. The
 * `currentUser` signal drives the template â€” it updates on login/logout
 * without manual subscriptions.
 */
@Component({
  selector: 'app-nav-bar',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    RouterLinkActive,
    NgbCollapseModule,
    NgbDropdownModule,
    BrandLogoComponent,
  ],
  templateUrl: './nav-bar.component.html',
  styleUrl: './nav-bar.component.css',
})
export class NavBarComponent {
  protected readonly auth = inject(AuthService);
  protected isMenuCollapsed = true;

  /** Initials displayed in the avatar circle (max 2 chars). */
  protected readonly initials = computed<string>(() => {
    const email = this.auth.currentUser()?.email ?? '';
    const local = email.split('@')[0] ?? '';
    if (!local) return '?';
    const parts = local.split(/[._-]+/).filter(Boolean);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return local.slice(0, 2).toUpperCase();
  });

  /** Friendly version of the role label, e.g. "FinanceOfficer" â†’ "Finance Officer". */
  protected readonly roleLabel = computed<string>(() => {
    const role = this.auth.currentUser()?.role ?? '';
    return role.replace(/([a-z])([A-Z])/g, '$1 $2');
  });

  logout(): void {
    this.auth.logout();
  }
}
