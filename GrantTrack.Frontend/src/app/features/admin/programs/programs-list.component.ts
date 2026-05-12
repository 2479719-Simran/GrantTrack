import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import {
  CreateProgramRequest,
  GetProgram,
  UpdateProgramRequest,
} from '../../../core/models/program.models';
import { AdminProgramService } from '../services/admin-program.service';
import { ToastService } from '../../../shared/services/toast.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { ProgramFormModalComponent } from './program-form-modal.component';

/**
 * Admin "Programs" page. Lists every grant program with budget + window
 * + status, with create/edit/delete actions and client-side filters.
 *
 * The backend has a POST /FilterPrograms endpoint, but it adds little
 * over filtering the loaded list client-side, so we only call it when the
 * admin explicitly clicks "Apply server filter" (lets us prove the
 * endpoint works while keeping the typical UX snappy).
 */
@Component({
  selector: 'app-programs-list',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe, DecimalPipe, PageHeaderComponent],
  templateUrl: './programs-list.component.html',
  styleUrl: './programs-list.component.css',
})
export class ProgramsListComponent implements OnInit {
  private readonly api = inject(AdminProgramService);
  private readonly toast = inject(ToastService);
  private readonly modal = inject(NgbModal);
  private readonly confirm = inject(ConfirmDialogService);

  protected readonly programs = signal<GetProgram[]>([]);
  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);

  // Filter state
  protected readonly search = signal('');
  protected readonly statusFilter = signal<'' | 'active' | 'inactive'>('');
  protected readonly fromDate = signal<string>('');
  protected readonly toDate = signal<string>('');

  protected readonly visiblePrograms = computed<GetProgram[]>(() => {
    const q = this.search().trim().toLowerCase();
    const status = this.statusFilter();
    const from = this.fromDate();
    const to = this.toDate();

    return this.programs().filter(p => {
      if (q) {
        const hay = `${p.name} ${p.description}`.toLowerCase();
        if (!hay.includes(q)) return false;
      }
      if (status === 'active' && !p.status) return false;
      if (status === 'inactive' && p.status) return false;
      if (from && p.startDate && new Date(p.startDate) < new Date(from)) return false;
      if (to && p.endDate && new Date(p.endDate) > new Date(to)) return false;
      return true;
    });
  });

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.api.getAll().subscribe({
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

  applyServerFilter(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.api.filter({
      // FilterProgramsDto.Status is a string ("true"/"false") on the BE.
      status: this.statusFilter() === ''
        ? null
        : (this.statusFilter() === 'active' ? 'true' : 'false'),
      startDate: this.fromDate() || null,
      endDate: this.toDate() || null,
    }).subscribe({
      next: list => {
        this.programs.set(list);
        this.loading.set(false);
        this.toast.info(`Server returned ${list.length} matching program(s).`);
      },
      error: (err: Error) => {
        this.loadError.set(err.message);
        this.loading.set(false);
      },
    });
  }

  clearFilters(): void {
    this.search.set('');
    this.statusFilter.set('');
    this.fromDate.set('');
    this.toDate.set('');
  }

  openCreate(): void {
    const ref = this.modal.open(ProgramFormModalComponent, { centered: true, size: 'lg', backdrop: 'static' });
    ref.componentInstance.program = null;
    ref.result.then(
      (payload: CreateProgramRequest) => {
        if (!payload) return;
        this.api.create(payload).subscribe({
          next: () => {
            this.toast.success(`Program "${payload.name}" created.`);
            this.refresh();
          },
          error: (err: Error) => this.toast.error(err.message),
        });
      },
      () => { /* dismissed */ },
    );
  }

  openEdit(program: GetProgram): void {
    const ref = this.modal.open(ProgramFormModalComponent, { centered: true, size: 'lg', backdrop: 'static' });
    ref.componentInstance.program = program;
    ref.result.then(
      (payload: UpdateProgramRequest) => {
        if (!payload) return;
        this.api.update(program.programId, payload).subscribe({
          next: () => {
            this.toast.success(`Program "${payload.name}" updated.`);
            this.refresh();
          },
          error: (err: Error) => this.toast.error(err.message),
        });
      },
      () => { /* dismissed */ },
    );
  }

  async deleteProgram(p: GetProgram): Promise<void> {
    const ok = await this.confirm.confirm({
      title: 'Delete program?',
      message: `<strong>${p.name}</strong> will be permanently removed. Linked applications, reviews and disbursements will also be impacted.`,
      confirmText: 'Delete',
      variant: 'danger',
      icon: 'bi-trash',
    });
    if (!ok) return;

    this.api.delete(p.programId).subscribe({
      next: () => {
        this.toast.success(`Program "${p.name}" deleted.`);
        this.refresh();
      },
      error: (err: Error) => this.toast.error(err.message),
    });
  }
}
