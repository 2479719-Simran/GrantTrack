import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-forbidden',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="container py-5">
      <div class="row justify-content-center">
        <div class="col-md-7 col-lg-5 text-center">
          <div class="card border-0 shadow-sm p-4 p-md-5">
            <div class="forbidden-icon mx-auto mb-3">
              <i class="bi bi-shield-lock"></i>
            </div>
            <h1 class="h3 fw-bold mb-2">Access denied</h1>
            <p class="text-body-secondary mb-4">
              You don't have permission to view this page. If you believe
              this is a mistake, please contact your administrator.
            </p>
            <a routerLink="/home" class="btn btn-primary">
              <i class="bi bi-arrow-left me-1"></i> Back to home
            </a>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .forbidden-icon {
      width: 72px;
      height: 72px;
      border-radius: 18px;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      background: rgba(220,38,38,.1);
      color: #dc2626;
      font-size: 2rem;
    }
  `],
})
export class ForbiddenComponent {}
