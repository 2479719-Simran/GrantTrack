// Mirrors review-related DTOs:
//   GrantTrack/Dto/ReviewDtos/BulkAssignmentDto
//   GrantTrack/Dto/ReviewFilterDto/*
//   GrantTrack/Dto/RecommendationDto/*
import { ReviewDecision } from './enums';

// --- Bulk assignment (Admin) -----------------------------------------------
export interface AssignmentItem {
  applicationId: number;
  reviewerId: number;
  score: number;
  comments: string;
}

export interface BulkAssignmentRequest {
  assignments: AssignmentItem[];
}

// --- Reviewer's assigned-reviews list --------------------------------------
export interface ReviewFilterRequest {
  reviewerId: number;
  pageNumber?: number;       // default 1 server-side
  pageSize?: number;         // default 10
  decision?: ReviewDecision | null;
}

export interface ReviewFilterResponse {
  reviewerId: number;
  pageNumber: number;
  pageSize: number;
  reviewId: number;
  applicationId: number;
  holderName: string;
  decision: ReviewDecision | null;
}

// --- Reviewer recommendation (POST /api/v1/recommendation/recommendation) --
export interface RecommendationRequest {
  applicationId: number;
  reviewerId: number;
  decision: ReviewDecision;
  notes: string;
  score: number;             // 1..10 — RegEx-validated server-side
  comments: string;
}

export interface RecommendationResponse {
  recommendationId: number;
  applicationId: number;
  reviewerName: string;
  decision: ReviewDecision;
  notes: string;
  date: string;
}
