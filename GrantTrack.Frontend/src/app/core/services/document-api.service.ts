import { HttpClient, HttpErrorResponse, HttpEvent, HttpEventType } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, filter, map, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AppDocument } from '../models/document.models';

/** Progress event emitted while a file is uploading. */
export interface UploadProgress {
  kind: 'progress';
  percent: number;        // 0..100
}
/** Final response when the upload completes. */
export interface UploadComplete {
  kind: 'complete';
  document: AppDocument;
}

export type UploadEvent = UploadProgress | UploadComplete;

/**
 * Generic API client for the document endpoints under
 * /api/v1/applications/{applicationId}/documents. Lives in core because
 * multiple roles use a subset of these methods:
 *
 *   • Applicant — list, upload, delete, download   (owns the application)
 *   • Reviewer  — list, download                   (assigned to the application)
 *
 * The backend enforces the per-method role + ownership/assignment check;
 * this client is just the wire format.
 */
@Injectable({ providedIn: 'root' })
export class DocumentApiService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiUrl;

  /** GET list. */
  list(applicationId: number): Observable<AppDocument[]> {
    return this.http
      .get<AppDocument[]>(`${this.base}/applications/${applicationId}/documents`)
      .pipe(catchError(this.toError));
  }

  /**
   * POST upload. Reports progress so the UI can show a per-file bar.
   * Emits a stream of UploadProgress events ending in UploadComplete.
   */
  upload(applicationId: number, file: File, docType: string | null): Observable<UploadEvent> {
    const form = new FormData();
    form.append('file', file, file.name);
    if (docType) form.append('docType', docType);

    return this.http.post<AppDocument>(
      `${this.base}/applications/${applicationId}/documents`,
      form,
      { reportProgress: true, observe: 'events' as const },
    ).pipe(
      filter((e: HttpEvent<AppDocument>) =>
        e.type === HttpEventType.UploadProgress || e.type === HttpEventType.Response),
      map((e: HttpEvent<AppDocument>): UploadEvent | null => {
        if (e.type === HttpEventType.UploadProgress) {
          const total = e.total ?? 0;
          const loaded = e.loaded ?? 0;
          const percent = total > 0 ? Math.round((loaded / total) * 100) : 0;
          return { kind: 'progress', percent };
        }
        if (e.type === HttpEventType.Response && e.body) {
          return { kind: 'complete', document: e.body };
        }
        return null;
      }),
      filter((e): e is UploadEvent => e !== null),
      catchError(this.toError),
    );
  }

  /** DELETE a document. */
  delete(applicationId: number, documentId: number): Observable<void> {
    return this.http
      .delete<void>(`${this.base}/applications/${applicationId}/documents/${documentId}`)
      .pipe(catchError(this.toError));
  }

  /**
   * Triggers a file save dialog in the browser. Because our auth interceptor
   * adds the Bearer header, we can't just point an <a href> at the URL —
   * the browser would navigate without the header and get a 401. So we
   * fetch the body as a blob, build an object URL, and click a hidden anchor.
   */
  download(doc: AppDocument): Observable<void> {
    const url = `${this.base}/applications/${doc.applicationId}/documents/${doc.documentId}/download`;
    return this.http.get(url, { responseType: 'blob' as const }).pipe(
      map(blob => {
        const objectUrl = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = objectUrl;
        a.download = doc.fileName;
        document.body.appendChild(a);
        a.click();
        a.remove();
        setTimeout(() => URL.revokeObjectURL(objectUrl), 1000);
      }),
      catchError(this.toError),
    );
  }

  private toError(err: HttpErrorResponse) {
    const msg = (typeof err.error === 'string' && err.error)
      || err.error?.error
      || err.error?.message
      || `Request failed (HTTP ${err.status}).`;
    return throwError(() => new Error(msg));
  }
}
