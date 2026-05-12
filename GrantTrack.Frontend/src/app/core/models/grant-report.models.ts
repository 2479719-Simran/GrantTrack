// Mirrors GrantTrack/Dto/GrantReportDtos/* and the GrantReport entity.
import { ReportStatus } from './enums';

export interface GrantReport {
  grantReportId: number;
  applicationId: number;
  scope: string;
  metrics: string;
  status: ReportStatus | string;     // "Draft" | "Submitted" | "Returned" | "Verified"
  submittedDate: string;
  notes: string | null;
  evidenceDocumentPath: string | null;
}

export interface GrantReportRequest {
  applicationId: number;
  scope: string;
  metrics: string;
  notes?: string | null;
  evidenceDocumentPath?: string | null;
}
