// Mirrors GrantTrack/Dto/AdminDtos/*.
import { ReviewDecision } from './enums';

export interface AdminApplicationListRow {
  applicationId: number;
  applicantId: number;
  applicantName: string;
  applicantEmail: string;
  programId: number;
  programName: string;
  status: string;
  submittedDate: string;

  reviewerCount: number;
  recommendationCount: number;
  documentCount: number;
  finalDecision: string | null;          // "Approved" | "Rejected" | null
  decisionDate: string | null;
}

export interface AdminReviewerInfo {
  reviewId: number;
  reviewerId: number;
  reviewerName: string;
  reviewerEmail: string;
  score: number;
  comments: string;
  assignedDate: string;
  hasSubmittedRecommendation: boolean;
}

export interface AdminRecommendationInfo {
  recommendationId: number;
  reviewerId: number;
  reviewerName: string;
  decision: ReviewDecision;
  notes: string;
  date: string;
}

export interface AdminDecisionInfo {
  decisionId: number;
  approverId: number;
  approverName: string;
  approverEmail: string;
  decisionValue: string;                 // "Approved" | "Rejected"
  notes: string;
  date: string;
}

export interface AdminDocumentInfo {
  documentId: number;
  docType: string;
  fileName: string;
  downloadUrl: string;
}

export interface AdminApplicationDetail {
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
  programStartDate: string;
  programEndDate: string;
  programBudget: number;

  reviewers: AdminReviewerInfo[];
  recommendations: AdminRecommendationInfo[];
  decision: AdminDecisionInfo | null;
  documents: AdminDocumentInfo[];
}
