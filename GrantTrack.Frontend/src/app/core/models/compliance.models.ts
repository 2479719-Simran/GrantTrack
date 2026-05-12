// Mirrors GrantTrack/Dto/ComplianceDtos/* and the existing
// ComplianceCheckDtos used by the schedule + complete endpoints.
import { ComplianceType, ComplianceResult } from './enums';

export interface ComplianceApplicationListRow {
  applicationId: number;
  applicantId: number;
  applicantName: string;
  applicantEmail: string;
  programId: number;
  programName: string;
  decisionDate: string;

  hasFinancialCheck: boolean;
  hasOperationalCheck: boolean;
  checkCount: number;
  verifiedCount: number;
  flaggedCount: number;
  scheduledCount: number;

  /** "Draft" | "Submitted" | "Returned" | "Verified" | null when no GrantReport. */
  reportStatus: string | null;
}

export interface ComplianceCheckInfo {
  checkId: number;
  applicationId: number;
  type: string;          // "Financial" | "Operational"
  result: string;        // raw enum: "Completed" | "Flagged"
  date: string;
  notes: string;
  /** Derived UX stage so the FE can show the right pill / actions. */
  stage: 'Scheduled' | 'Verified' | 'Returned' | string;
}

export interface ComplianceApplicationDetail {
  applicationId: number;
  status: string;
  submittedDate: string;

  applicantId: number;
  applicantName: string;
  applicantEmail: string;
  applicantPhone: string;

  programId: number;
  programName: string;
  programDescription: string;
  programBudget: number;
  programStartDate: string;
  programEndDate: string;

  approverId: number | null;
  approverName: string | null;
  decisionNotes: string | null;
  decisionDate: string | null;

  grantReportId: number | null;
  reportStatus: string | null;
  reportScope: string | null;
  reportNotes: string | null;
  reportSubmittedDate: string | null;

  checks: ComplianceCheckInfo[];
}

// --- Request shapes for the existing ComplianceCheckController -------------

export interface ScheduleCheckRequest {
  applicationId: number;
  type: ComplianceType;        // "Financial" | "Operational"
  notes: string;
}

export interface CompleteCheckRequest {
  result: ComplianceResult;    // "Completed" | "Flagged"
  notes: string;
}
