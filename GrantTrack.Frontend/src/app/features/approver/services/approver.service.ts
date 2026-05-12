import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  ApproverRecommendation,
  AwaitingDecision,
} from '../../../core/models/approver.models';
import { DecisionRequest } from '../../../core/models/decision.models';

/**
 * Approver-only API surface:
 *   GET  /api/v1/approvals/awaiting                   – queue of pending apps
 *   GET  /api/v1/approvals/{id}/recommendations       – per-app recs
 *   POST /api/v1/decision                             – record final decision
 */
@Injectable({ providedIn: 'root' })
export class ApproverService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiUrl;

  awaiting(): Observable<AwaitingDecision[]> {
    return this.http
      .get<AwaitingDecision[]>(`${this.base}/approvals/awaiting`)
      .pipe(catchError(this.toError));
  }

  recommendations(applicationId: number): Observable<ApproverRecommendation[]> {
    return this.http
      .get<ApproverRecommendation[]>(`${this.base}/approvals/${applicationId}/recommendations`)
      .pipe(catchError(this.toError));
  }

  /**
   * POST /api/v1/decision — DecisionController returns a plain text 201
   * ("Decision recorded successfully.") so we use responseType:text and wrap
   * the body into a uniform { message } shape for callers.
   */
  decide(req: DecisionRequest): Observable<{ message: string }> {
    return this.http
      .post(`${this.base}/decision`, req, { responseType: 'text' as const })
      .pipe(
        map(text => ({ message: (text || 'Decision recorded.').replace(/^"|"$/g, '') })),
        catchError(this.toError),
      );
  }

  private toError(err: HttpErrorResponse) {
    // When responseType is 'text', err.error arrives as a string even when
    // the server actually returned a JSON envelope (e.g. { "message": "..." }).
    // Try to parse it so structured error bodies display cleanly instead of
    // showing the raw braces in the toast.
    let body: any = err.error;
    if (typeof body === 'string') {
      const trimmed = body.trim();
      if (trimmed.startsWith('{') || trimmed.startsWith('[')) {
        try { body = JSON.parse(trimmed); } catch { /* leave as string */ }
      }
    }
    const msg = (typeof body === 'string' && body)
      || body?.error
      || body?.message
      || (body?.errors && Object.values<string[]>(body.errors)[0]?.[0])
      || `Request failed (HTTP ${err.status}).`;
    return throwError(() => new Error(msg));
  }
}
