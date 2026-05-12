// Mirrors GrantTrack/Dto/ApproverDtos/*.
import { ReviewDecision } from './enums';

export interface AwaitingDecision {
  applicationId: number;
  applicantId: number;
  applicantName: string;
  applicantEmail: string;
  programId: number;
  programName: string;
  submittedDate: string;
  status: string;

  recommendationCount: number;
  approvedCount: number;
  rejectedCount: number;
  averageScore: number;          // 0 when no reviews scored yet
}

export interface ApproverRecommendation {
  recommendationId: number;
  applicationId: number;
  reviewerId: number;
  reviewerName: string;
  reviewerEmail: string;
  decision: ReviewDecision;
  recommendationNotes: string;
  score: number;                  // 1..10
  reviewComments: string;
  date: string;
}
