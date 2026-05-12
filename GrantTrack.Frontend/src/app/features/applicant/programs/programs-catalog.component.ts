import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { GetProgram } from '../../../core/models/program.models';
import { ApplicantProgramService } from '../services/applicant-program.service';
import { ApplicantService } from '../services/applicant.service';
import { ToastService } from '../../../shared/services/toast.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { ApplyProgramModalComponent } from './apply-program-modal.component';

/**
 * Browse-and-apply screen for the Applicant role. Lists every active grant
 * program as a card and exposes one CTA per program: "Apply".
 *
 * Cards already-applied-to render a disabled "Already applied" pill plus
 * a link to the My Applications page so the applicant can find their draft.
 */
@Component({
  selector: 'app-programs-catalog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DatePipe,
    DecimalPipe,
    RouterLink,
    PageHeaderComponent,
  ],
  templateUrl: './programs-catalog.component.html',
  styleUrl: './programs-catalog.component.css',
})
export class ProgramsCatalogComponent implements OnInit {
  private readonly programApi = inject(ApplicantProgramService);
  private readonly applicantApi = inject(ApplicantService);
  private readonly modal = inject(NgbModal);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  protected readonly programs = signal<GetProgram[]>([]);
  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);

  protected readonly search = signal('');

  protected readonly visiblePrograms = computed<GetProgram[]>(() => {
    const q = this.search().trim().toLowerCase();
    if (!q) return this.programs();
    return this.programs().filter(p =>
      `${p.name} ${p.description}`.toLowerCase().includes(q));
  });

  /** Set of program ids the applicant already has an application for. */
  protected readonly appliedIds = computed<Set<number>>(() =>
    new Set(this.applicantApi.applications().map(a => a.programId)));

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.programApi.getActive().subscribe({
      next: list => {
        this.programs.set(list);
        this.loading.set(false);
      },
      error: (err: Error) => {
        this.loadError.set(err.message);
        this.loading.set(false);
      },
    });
  }

  apply(program: GetProgram): void {
    if (this.appliedIds().has(program.programId)) {
      this.toast.info('You already have an application for this program. Open it from My Applications.');
      return;
    }

    const ref = this.modal.open(ApplyProgramModalComponent, {
      centered: true,
      size: 'lg',
      backdrop: 'static',
    });
    ref.componentInstance.program = program;
    ref.result.then(
      (result?: { applicationId: number }) => {
        if (!result) return;
        this.toast.success(`Draft created for "${program.name}". Upload your documents next.`);
        // Drop the applicant on the new detail page where they upload docs
        // and submit when ready.
        this.router.navigate(['/applicant/applications', result.applicationId]);
      },
      () => { /* dismissed */ },
    );
  }

  /** Days remaining until the program closes â€” used for urgency badges. */
  protected daysLeft(p: GetProgram): number {
    const end = new Date(p.endDate).getTime();
    const now = Date.now();
    return Math.max(0, Math.ceil((end - now) / (1000 * 60 * 60 * 24)));
  }
}
