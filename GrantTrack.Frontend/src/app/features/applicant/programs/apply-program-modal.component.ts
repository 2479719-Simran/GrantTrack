import { Component, Input, inject, signal } from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { GetProgram } from '../../../core/models/program.models';
import { ApplicantService } from '../services/applicant.service';

/**
 * Apply-to-program confirmation. Creates a draft application then resolves
 * with the new application id so the host page can route to the detail
 * view for document upload + submit.
 *
 * The previous version offered a "submit immediately" path, but since the
 * applicant now has to upload supporting documents, we always go through
 * the detail page where they can upload before submitting.
 */
@Component({
  selector: 'app-apply-program-modal',
  standalone: true,
  imports: [CommonModule, DatePipe, DecimalPipe],
  templateUrl: './apply-program-modal.component.html',
  styleUrl: './apply-program-modal.component.css',
})
export class ApplyProgramModalComponent {
  protected readonly active = inject(NgbActiveModal);
  private readonly api = inject(ApplicantService);

  @Input({ required: true }) program!: GetProgram;

  protected readonly busy = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  confirm(): void {
    this.errorMessage.set(null);
    this.busy.set(true);

    this.api.createDraft(this.program.programId, this.program.name).subscribe({
      next: res => this.active.close({ applicationId: res.applicationId }),
      error: (err: Error) => {
        this.busy.set(false);
        this.errorMessage.set(err.message);
      },
    });
  }
}
