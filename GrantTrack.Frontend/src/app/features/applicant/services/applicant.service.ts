import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { Observable, catchError, tap, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApplicationResponse } from '../../../core/models/application.models';
import { ApplicationStatus } from '../../../core/models/enums';
import { AuthService } from '../../../core/services/auth.service';

/**
 * One row in the applicant's local "my applications" cache. Mirrors the
 * server response shape but adds the program name we captured at apply
 * time, so the My Applications page has something to display.
 *
 * Why local cache: the backend has no GET /applications/mine endpoint, and
 * no GET /applications/{id} either. Until those exist, the applicant's
 * own client is the only place that knows their draft history. We key
 * storage by user id so multiple users on the same machine don't collide.
 */
export interface CachedApplication {
  applicationId: number;
  programId: number;
  programName: string;
  applicantId: number;
  status: ApplicationStatus | string;
  submittedDate: string | null;
  /** Frontend timestamp captured when we first stored the row. */
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class ApplicantService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly base = `${environment.apiUrl}/applications`;

  /** Reactive view of the current applicant's cached apps. */
  private readonly _cache = signal<CachedApplication[]>(this.readCache());
  readonly applications = this._cache.asReadonly();

  readonly draftCount = computed(() =>
    this._cache().filter(a => a.status === 'Draft').length);
  readonly submittedCount = computed(() =>
    this._cache().filter(a => a.status !== 'Draft').length);

  constructor() {
    // The service is a root-scoped singleton, so when user A signs out and
    // user B signs in we need to swap to user B's storage namespace. The
    // effect re-reads from localStorage whenever the current user id changes.
    //
    // `allowSignalWrites: true` is the documented opt-in for effects that
    // *intentionally* need to write to a signal. Newer Angular versions
    // make this strict by default — without the flag we'd get an NG0600
    // runtime error. The intent here is exactly the side-effect Angular
    // wants us to acknowledge: cache-swap on identity change.
    let lastUserId: number | undefined = this.auth.currentUser()?.userId;
    effect(() => {
      const id = this.auth.currentUser()?.userId;
      if (id !== lastUserId) {
        lastUserId = id;
        this._cache.set(this.readCache());
      }
    }, { allowSignalWrites: true });
  }

  /** POST /api/v1/applications — creates a new Draft application. */
  createDraft(programId: number, programName: string): Observable<ApplicationResponse> {
    return this.http
      .post<ApplicationResponse>(this.base, { programId })
      .pipe(
        tap(res => {
          this.upsertCache({
            applicationId: res.applicationId,
            programId: res.programId,
            programName,
            applicantId: res.applicantId,
            status: res.status,
            submittedDate: res.submittedDate,
            createdAt: new Date().toISOString(),
          });
        }),
        catchError(this.toError),
      );
  }

  /**
   * GET /api/v1/applications/{id} — refresh the cached status from the
   * server. The local cache only updates on submit, so this is how we
   * pick up downstream state changes (e.g. the Approver flipping the
   * status to Approved or Rejected).
   *
   * Updates the in-memory + persisted cache while preserving FE-only
   * fields like programName and createdAt.
   */
  refreshFromServer(applicationId: number): Observable<ApplicationResponse> {
    return this.http
      .get<ApplicationResponse>(`${this.base}/${applicationId}`)
      .pipe(
        tap(res => {
          this.upsertCache(prev => ({
            ...prev,
            status: res.status,
            submittedDate: res.submittedDate,
          }), applicationId);
        }),
        catchError(this.toError),
      );
  }

  /** POST /api/v1/applications/{id}/submit — flips status from Draft → Submitted. */
  submit(applicationId: number): Observable<ApplicationResponse> {
    return this.http
      .post<ApplicationResponse>(`${this.base}/${applicationId}/submit`, {})
      .pipe(
        tap(res => {
          this.upsertCache(prev => ({
            ...prev,
            status: res.status,
            submittedDate: res.submittedDate,
          }), applicationId);
        }),
        catchError(this.toError),
      );
  }

  /** Drop a row from the local cache (e.g. user no longer wants to track it). */
  removeFromCache(applicationId: number): void {
    const next = this._cache().filter(a => a.applicationId !== applicationId);
    this._cache.set(next);
    this.writeCache(next);
  }

  /** Wipe the entire cache for the current user — used on logout. */
  clearCache(): void {
    const key = this.cacheKey();
    if (key) localStorage.removeItem(key);
    this._cache.set([]);
  }

  // -------------------------- internals ------------------------------------

  private cacheKey(): string | null {
    const id = this.auth.currentUser()?.userId;
    return id ? `gt.apps.${id}` : null;
  }

  private readCache(): CachedApplication[] {
    const key = this.cacheKey();
    if (!key) return [];
    const raw = localStorage.getItem(key);
    if (!raw) return [];
    try {
      const parsed = JSON.parse(raw);
      return Array.isArray(parsed) ? (parsed as CachedApplication[]) : [];
    } catch {
      return [];
    }
  }

  private writeCache(rows: CachedApplication[]): void {
    const key = this.cacheKey();
    if (!key) return;
    localStorage.setItem(key, JSON.stringify(rows));
  }

  /**
   * Insert a new row, OR mutate an existing row matched by id.
   * The two-overload signature keeps callsites readable.
   */
  private upsertCache(entry: CachedApplication): void;
  private upsertCache(
    update: (prev: CachedApplication) => CachedApplication,
    id: number,
  ): void;
  private upsertCache(
    arg: CachedApplication | ((prev: CachedApplication) => CachedApplication),
    id?: number,
  ): void {
    const list = [...this._cache()];

    if (typeof arg === 'function') {
      const idx = list.findIndex(a => a.applicationId === id);
      if (idx >= 0) list[idx] = arg(list[idx]);
    } else {
      const idx = list.findIndex(a => a.applicationId === arg.applicationId);
      if (idx >= 0) list[idx] = { ...list[idx], ...arg };
      else list.unshift(arg);
    }

    this._cache.set(list);
    this.writeCache(list);
  }

  /** Decode the most useful error message we can find in the response. */
  private toError(err: HttpErrorResponse) {
    const msg = (typeof err.error === 'string' && err.error)
      || err.error?.error
      || err.error?.message
      || (err.error?.errors && Object.values<string[]>(err.error.errors)[0]?.[0])
      || `Request failed (HTTP ${err.status}).`;
    return throwError(() => new Error(msg));
  }
}
