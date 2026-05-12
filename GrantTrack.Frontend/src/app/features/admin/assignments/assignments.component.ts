import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  FormArray,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ViewUser } from '../../../core/models/user.models';
import { AdminApplicationListRow } from '../../../core/models/admin-application.models';
import { AdminUserService } from '../services/admin-user.service';
import { AdminAssignmentService } from '../services/admin-assignment.service';
import { AdminApplicationService } from '../services/admin-application.service';
import { ToastService } from '../../../shared/services/toast.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

/**
 * One applicant grouped with their not-yet-decided applications, used to
 * populate the primary picker on this page.
 */
interface ApplicantBucket {
  applicantId: number;
  applicantName: string;
  applicantEmail: string;
  applications: AdminApplicationListRow[];
}

/**
 * Reviewer assignments â€” applicant-first workflow.
 *
 * The admin's mental model is "I want to assign a reviewer to <applicant>'s
 * submission", not "to application id 1042". So the page exposes an
 * Applicant picker as the primary choice. When that applicant has only one
 * pending application we auto-resolve it; when they have several, a second
 * picker appears.
 *
 * The BE still requires applicationId (see ReviewService.BulkAssignReviewersAsync
 * â€” it validates against the Applications table and writes Review rows keyed
 * by ApplicationId), so the FE passes applicationId as before. Only the
 * UX changed.
 */
@Component({
  selector: 'app-assignments',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, PageHeaderComponent],
  templateUrl: './assignments.component.html',
  styleUrl: './assignments.component.css',
})
export class AssignmentsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly userApi = inject(AdminUserService);
  private readonly appApi = inject(AdminApplicationService);
  private readonly api = inject(AdminAssignmentService);
  private readonly toast = inject(ToastService);

  protected readonly users = signal<ViewUser[]>([]);
  protected readonly loadingUsers = signal(false);
  protected readonly userLoadError = signal<string | null>(null);

  protected readonly applications = signal<AdminApplicationListRow[]>([]);
  protected readonly loadingApps = signal(false);
  protected readonly appLoadError = signal<string | null>(null);

  protected readonly submitting = signal(false);

  /** Active reviewers â€” populated from the user list. */
  protected readonly reviewers = computed<ViewUser[]>(() =>
    this.users().filter(u => u.status && u.role === 'Reviewer'));

  /**
   * Group every assignable application by applicant, dropping decided ones.
   * This is what the primary picker (column 1) renders.
   */
  protected readonly applicantBuckets = computed<ApplicantBucket[]>(() => {
    const map = new Map<number, ApplicantBucket>();
    for (const a of this.applications()) {
      if (a.finalDecision) continue;        // already decided â€” nothing to assign
      if (!map.has(a.applicantId)) {
        map.set(a.applicantId, {
          applicantId: a.applicantId,
          applicantName: a.applicantName,
          applicantEmail: a.applicantEmail,
          applications: [],
        });
      }
      map.get(a.applicantId)!.applications.push(a);
    }
    return Array.from(map.values())
      .sort((x, y) => (x.applicantName || x.applicantEmail)
        .localeCompare(y.applicantName || y.applicantEmail));
  });

  protected readonly form: FormGroup = this.fb.nonNullable.group({
    assignments: this.fb.array([this.buildRow()]),
  });

  protected get rows(): FormArray<FormGroup> {
    return this.form.get('assignments') as FormArray<FormGroup>;
  }

  ngOnInit(): void {
    // Load reviewers + applications in parallel â€” fail-soft per stream.
    this.loadingUsers.set(true);
    this.userApi.getAll().subscribe({
      next: list => { this.users.set(list); this.loadingUsers.set(false); },
      error: (err: Error) => {
        this.userLoadError.set(err.message);
        this.loadingUsers.set(false);
        this.toast.error(`Could not load reviewers: ${err.message}`);
      },
    });

    this.loadingApps.set(true);
    this.appApi.list().subscribe({
      next: list => { this.applications.set(list); this.loadingApps.set(false); },
      error: (err: Error) => {
        this.appLoadError.set(err.message);
        this.loadingApps.set(false);
        this.toast.error(`Could not load applications: ${err.message}`);
      },
    });
  }

  addRow(): void { this.rows.push(this.buildRow()); }

  removeRow(i: number): void {
    if (this.rows.length === 1) {
      this.rows.at(0).reset({
        applicantId: null, applicationId: null, reviewerId: null, score: 0, comments: '',
      });
      return;
    }
    this.rows.removeAt(i);
  }

  /**
   * When the admin picks an applicant we either auto-resolve to their only
   * application (one less click) or wipe the applicationId so the second
   * picker forces a choice.
   */
  onApplicantChange(rowIndex: number, applicantId: number | null): void {
    const row = this.rows.at(rowIndex);
    row.patchValue({ applicantId });
    if (applicantId === null) {
      row.patchValue({ applicationId: null });
      return;
    }
    const bucket = this.applicantBuckets().find(b => b.applicantId === applicantId);
    if (!bucket) {
      row.patchValue({ applicationId: null });
      return;
    }
    if (bucket.applications.length === 1) {
      row.patchValue({ applicationId: bucket.applications[0].applicationId });
    } else {
      row.patchValue({ applicationId: null });        // force explicit pick
    }
  }

  /** Apps owned by the row's currently-picked applicant (for the sub-picker). */
  protected appsFor(applicantId: number | null): AdminApplicationListRow[] {
    if (applicantId == null) return [];
    return this.applicantBuckets().find(b => b.applicantId === applicantId)?.applications ?? [];
  }

  /** The application object for a given id (used to render the inline summary). */
  protected appById(id: number | null): AdminApplicationListRow | undefined {
    if (id == null) return undefined;
    return this.applications().find(a => a.applicationId === id);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    // BE only consumes applicationId/reviewerId/score/comments â€” strip the
    // applicantId field we use purely for UX.
    const assignments = this.rows.controls.map(c => {
      const v = c.getRawValue();
      return {
        applicationId: v.applicationId as number,
        reviewerId: v.reviewerId as number,
        score: v.score as number,
        comments: v.comments as string,
      };
    });

    // Sanity: warn on duplicates the BE would reject.
    const seen = new Set<string>();
    for (const a of assignments) {
      const key = `${a.applicationId}|${a.reviewerId}`;
      if (seen.has(key)) {
        this.toast.warning(`Duplicate assignment in the batch: application #${a.applicationId} â†’ reviewer ${a.reviewerId}.`);
        return;
      }
      seen.add(key);
    }

    this.submitting.set(true);
    this.api.bulkAssign({ assignments }).subscribe({
      next: res => {
        this.submitting.set(false);
        this.toast.success(res?.message || `Assigned ${assignments.length} reviewer(s).`);
        this.rows.clear();
        this.rows.push(this.buildRow());
      },
      error: (err: Error) => {
        this.submitting.set(false);
        this.toast.error(err.message);
      },
    });
  }

  private buildRow(): FormGroup {
    return this.fb.nonNullable.group({
      // Primary picker â€” what the admin actually thinks about.
      applicantId: [null as number | null, [Validators.required]],
      // Resolved automatically when the applicant has one application,
      // otherwise picked from the sub-dropdown. Always required at submit.
      applicationId: [null as number | null, [Validators.required, Validators.min(1)]],
      reviewerId: [null as number | null, [Validators.required, Validators.min(1)]],
      score: [0],
      comments: [''],
    });
  }
}
