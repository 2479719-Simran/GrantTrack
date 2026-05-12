import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  AdminApplicationDetail,
  AdminDocumentInfo,
} from '../../../core/models/admin-application.models';
import { AdminApplicationService } from '../services/admin-application.service';
import { DocumentApiService } from '../../../core/services/document-api.service';
import { AppDocument } from '../../../core/models/document.models';
import { ToastService } from '../../../shared/services/toast.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { FundingPanelComponent } from '../../../shared/components/funding-panel/funding-panel.component';

/**
 * Admin's drill-in for a single application. Shows the full audit picture:
 *   â€¢ Header: program + applicant
 *   â€¢ Reviewers panel (assigned, score, has-recommended flag)
 *   â€¢ Recommendations panel (decisions submitted by each reviewer)
 *   â€¢ Final decision (if any) â€” name of approver, date, notes
 *   â€¢ Documents (downloadable â€” Admin has read-only access via the existing
 *     /api/v1/applications/{id}/documents/{docId}/download endpoint)
 */
@Component({
  selector: 'app-admin-application-detail',
  standalone: true,
  imports: [CommonModule, DatePipe, DecimalPipe, RouterLink, PageHeaderComponent, FundingPanelComponent],
  templateUrl: './application-detail.component.html',
  styleUrl: './application-detail.component.css',
})
export class AdminApplicationDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(AdminApplicationService);
  private readonly docApi = inject(DocumentApiService);
  private readonly toast = inject(ToastService);

  protected readonly applicationId = signal<number>(0);
  protected readonly detail = signal<AdminApplicationDetail | null>(null);
  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly downloadingDocId = signal<number | null>(null);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isFinite(id) || id <= 0) {
      this.router.navigateByUrl('/admin/applications');
      return;
    }
    this.applicationId.set(id);
    this.refresh();
  }

  refresh(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.api.getDetail(this.applicationId()).subscribe({
      next: d => {
        this.detail.set(d);
        this.loading.set(false);
      },
      error: (err: Error) => {
        this.loadError.set(err.message);
        this.loading.set(false);
      },
    });
  }

  download(d: AdminDocumentInfo): void {
    this.downloadingDocId.set(d.documentId);
    // The DocumentApiService expects an AppDocument shape â€” we have the same
    // fields under a slightly different DTO, so adapt before calling.
    const adapted: AppDocument = {
      documentId: d.documentId,
      applicationId: this.applicationId(),
      docType: d.docType,
      fileName: d.fileName,
      downloadUrl: d.downloadUrl,
    };
    this.docApi.download(adapted).subscribe({
      next: () => this.downloadingDocId.set(null),
      error: (err: Error) => {
        this.downloadingDocId.set(null);
        this.toast.error(err.message);
      },
    });
  }

  // ---------- helpers --------------------------------------------------

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

  protected decisionClass(d: string | undefined): string {
    switch (d) {
      case 'Approved': return 'bg-success-subtle text-success-emphasis';
      case 'Rejected': return 'bg-danger-subtle text-danger-emphasis';
      case 'Pending':  return 'bg-warning-subtle text-warning-emphasis';
      default:         return 'bg-secondary-subtle text-secondary-emphasis';
    }
  }

  protected initials(name: string, email: string): string {
    const source = (name && name.trim()) || (email?.split('@')[0] ?? '?');
    const parts = source.split(/[\s._-]+/).filter(Boolean);
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
    return source.slice(0, 2).toUpperCase();
  }

  protected fileIcon(name: string): string {
    const ext = name.split('.').pop()?.toLowerCase() ?? '';
    if (['pdf'].includes(ext)) return 'bi-file-earmark-pdf';
    if (['jpg', 'jpeg', 'png', 'gif'].includes(ext)) return 'bi-file-earmark-image';
    if (['doc', 'docx'].includes(ext)) return 'bi-file-earmark-word';
    if (['xls', 'xlsx'].includes(ext)) return 'bi-file-earmark-excel';
    if (['txt'].includes(ext)) return 'bi-file-earmark-text';
    return 'bi-file-earmark';
  }
}
