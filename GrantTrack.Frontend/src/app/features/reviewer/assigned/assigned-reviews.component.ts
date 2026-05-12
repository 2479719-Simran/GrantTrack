import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ReviewDecision } from '../../../core/models/enums';
import { ReviewFilterResponse } from '../../../core/models/review.models';
import { AuthService } from '../../../core/services/auth.service';
import { ReviewerService } from '../services/reviewer.service';
import { ToastService } from '../../../shared/services/toast.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { RecommendationModalComponent } from './recommendation-modal.component';

type Tab = 'Pending' | 'Approved' | 'Rejected' | 'All';

/**
 * Reviewer's "assigned reviews" list. Tabs filter by decision; the BE
 * returns the same payload, just narrowed.
 *
 * The BE's response doesn't carry total-count metadata, so we drive
 * paging by inspecting whether the current page came back full â€” if it did,
 * a "Next" page might exist; if it didn't, we're on the last page. This
 * is the same heuristic used in many "load-more" lists.
 */
@Component({
  selector: 'app-assigned-reviews',
  standalone: true,
  imports: [CommonModule, FormsModule, PageHeaderComponent],
  templateUrl: './assigned-reviews.component.html',
  styleUrl: './assigned-reviews.component.css',
})
export class AssignedReviewsComponent implements OnInit {
  private readonly api = inject(ReviewerService);
  private readonly auth = inject(AuthService);
  private readonly modal = inject(NgbModal);
  private readonly toast = inject(ToastService);

  protected readonly rows = signal<ReviewFilterResponse[]>([]);
  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);

  protected readonly tab = signal<Tab>('Pending');
  protected readonly pageNumber = signal(1);
  protected readonly pageSize = signal(10);

  /** True iff we received exactly pageSize rows on the last fetch. */
  protected readonly hasNextPage = computed(() =>
    this.rows().length === this.pageSize());

  protected readonly tabs: Tab[] = ['Pending', 'Approved', 'Rejected', 'All'];

  ngOnInit(): void {
    this.refresh();
  }

  setTab(t: Tab): void {
    if (this.tab() === t) return;
    this.tab.set(t);
    this.pageNumber.set(1);
    this.refresh();
  }

  prevPage(): void {
    if (this.pageNumber() <= 1) return;
    this.pageNumber.update(n => n - 1);
    this.refresh();
  }

  nextPage(): void {
    if (!this.hasNextPage()) return;
    this.pageNumber.update(n => n + 1);
    this.refresh();
  }

  refresh(): void {
    const reviewerId = this.auth.currentUser()?.userId;
    if (!reviewerId) {
      this.loadError.set('Not signed in.');
      return;
    }
    this.loading.set(true);
    this.loadError.set(null);

    const decisionFilter: ReviewDecision | undefined =
      this.tab() === 'All' ? undefined : (this.tab() as ReviewDecision);

    this.api.listAssigned({
      reviewerId,
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
      decision: decisionFilter,
    }).subscribe({
      next: list => {
        this.rows.set(list ?? []);
        this.loading.set(false);
      },
      error: (err: Error) => {
        this.loadError.set(err.message);
        this.loading.set(false);
      },
    });
  }

  openRecommendationModal(row: ReviewFilterResponse): void {
    const reviewerId = this.auth.currentUser()?.userId;
    if (!reviewerId) return;
    if (row.decision && row.decision !== 'Pending') {
      this.toast.info('This review already has a recommendation. Open the application to view it.');
      return;
    }

    const ref = this.modal.open(RecommendationModalComponent, {
      centered: true,
      size: 'xl',
      backdrop: 'static',
      scrollable: true,
    });
    ref.componentInstance.review = row;
    ref.componentInstance.reviewerId = reviewerId;

    ref.result.then(
      (result?: { decision: ReviewDecision; message: string }) => {
        if (!result) return;
        this.toast.success(result.message);
        // Optimistically patch the row so the user doesn't have to re-fetch
        // before seeing their decision land.
        this.rows.update(list =>
          list.map(r => r.reviewId === row.reviewId
            ? { ...r, decision: result.decision }
            : r));
        // Then re-fetch so the next refresh stays accurate (e.g. the row
        // disappears from the Pending tab if we're filtered there).
        this.refresh();
      },
      () => { /* dismissed */ },
    );
  }

  /** Bootstrap classes for the per-row decision pill. */
  protected decisionClass(d: ReviewDecision | null): string {
    switch (d) {
      case 'Approved': return 'bg-success-subtle text-success-emphasis';
      case 'Rejected': return 'bg-danger-subtle text-danger-emphasis';
      case 'Pending':  return 'bg-warning-subtle text-warning-emphasis';
      default:         return 'bg-secondary-subtle text-secondary-emphasis';
    }
  }
  protected decisionIcon(d: ReviewDecision | null): string {
    switch (d) {
      case 'Approved': return 'bi-check-circle';
      case 'Rejected': return 'bi-x-circle';
      case 'Pending':  return 'bi-hourglass-split';
      default:         return 'bi-dash-circle';
    }
  }

  protected initials(name: string): string {
    if (!name) return '?';
    const parts = name.split(/[\s._-]+/).filter(Boolean);
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
    return name.slice(0, 2).toUpperCase();
  }
}
