import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import {
  ApplicantService,
  CachedApplication,
} from '../services/applicant.service';
import { ToastService } from '../../../shared/services/toast.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

type FilterTab = 'all' | 'draft' | 'submitted';

/**
 * "My applications" â€” the applicant's view of everything they've started.
 *
 * Data is read from the local cache (no list-mine endpoint exists on the
 * BE), but on every page open we fan out and refresh each cached app via
 * GET /api/v1/applications/{id} so downstream state changes (an Approver
 * decision, etc.) flip the status pills here without the user having to
 * open each detail page.
 */
@Component({
  selector: 'app-my-applications',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DatePipe,
    RouterLink,
    PageHeaderComponent,
  ],
  templateUrl: './my-applications.component.html',
  styleUrl: './my-applications.component.css',
})
export class MyApplicationsComponent implements OnInit {
  protected readonly api = inject(ApplicantService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmDialogService);

  protected readonly tab = signal<FilterTab>('all');
  protected readonly refreshing = signal(false);

  protected readonly visible = computed<CachedApplication[]>(() => {
    const list = this.api.applications();
    switch (this.tab()) {
      case 'draft':     return list.filter(a => a.status === 'Draft');
      case 'submitted': return list.filter(a => a.status !== 'Draft');
      default:          return list;
    }
  });

  ngOnInit(): void {
    this.refreshAll();
  }

  /**
   * Fan out one GET per cached application to pick up downstream state
   * changes. Each successful response updates the cache via
   * ApplicantService.refreshFromServer.
   *
   * We capture failures (instead of swallowing them silently) and surface
   * one summary toast so the user can tell apart "nothing changed" from
   * "refresh failed because the BE isn't reachable." The most common cause
   * of failure here is the BE not being restarted after the new
   * GET /api/v1/applications/{id} endpoint was added.
   */
  refreshAll(): void {
    const apps = this.api.applications();
    if (apps.length === 0) return;

    this.refreshing.set(true);
    let failures = 0;
    let lastError: string | null = null;

    const calls = apps.map(a =>
      this.api.refreshFromServer(a.applicationId).pipe(
        catchError((err: Error) => {
          failures++;
          lastError = err.message;
          // eslint-disable-next-line no-console
          console.warn(`[MyApplications] refresh #${a.applicationId} failed:`, err.message);
          return of(null);
        }),
      ),
    );
    forkJoin(calls)
      .pipe(finalize(() => this.refreshing.set(false)))
      .subscribe(() => {
        if (failures > 0) {
          this.toast.warning(
            `Couldn't refresh ${failures} application(s) from the server: ${lastError}. ` +
            `Status pills may be stale until the backend is reachable.`,
          );
        }
      });
  }

  setTab(t: FilterTab): void { this.tab.set(t); }

  async stopTracking(app: CachedApplication): Promise<void> {
    const ok = await this.confirm.confirm({
      title: 'Stop tracking this application?',
      message: `This only removes <strong>${app.programName}</strong> from your local list. The application itself stays on the server. Continue?`,
      confirmText: 'Remove from list',
      variant: 'danger',
      icon: 'bi-x-circle',
    });
    if (!ok) return;
    this.api.removeFromCache(app.applicationId);
    this.toast.info('Removed from your local list.');
  }

  /** Bootstrap class chunk based on application status. */
  protected statusClass(status: string): string {
    switch (status) {
      case 'Draft':       return 'bg-secondary-subtle text-secondary-emphasis';
      case 'Submitted':   return 'bg-info-subtle text-info-emphasis';
      case 'UnderReview': return 'bg-warning-subtle text-warning-emphasis';
      case 'Approved':    return 'bg-success-subtle text-success-emphasis';
      case 'Rejected':    return 'bg-danger-subtle text-danger-emphasis';
      default:            return 'bg-secondary-subtle text-secondary-emphasis';
    }
  }
  protected statusIcon(status: string): string {
    switch (status) {
      case 'Draft':       return 'bi-file-earmark-text';
      case 'Submitted':   return 'bi-send-check';
      case 'UnderReview': return 'bi-hourglass-split';
      case 'Approved':    return 'bi-check-circle';
      case 'Rejected':    return 'bi-x-circle';
      default:            return 'bi-file-earmark';
    }
  }
}
