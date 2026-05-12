// Mirrors of the C# enums in GrantTrack.Domain.Entities.
// Backend serializes enums as strings (HasConversion<string>() — see
// GrantTrackDbContext.OnModelCreating), so we model them as string-literal
// unions everywhere they cross the wire.

export type UserRole =
  | 'Admin'
  | 'Applicant'
  | 'Reviewer'
  | 'Approver'
  | 'FinanceOfficer'
  | 'ComplianceOfficer';

export const ALL_ROLES: UserRole[] = [
  'Admin',
  'Applicant',
  'Reviewer',
  'Approver',
  'FinanceOfficer',
  'ComplianceOfficer',
];

export type ApplicationStatus =
  | 'Draft'
  | 'Submitted'
  | 'UnderReview'
  | 'Approved'
  | 'Rejected';

export type DecisionStatus = 'Approved' | 'Rejected';

export type ReviewDecision = 'Pending' | 'Approved' | 'Rejected';

export type DisbursementStatus =
  | 'Pending'
  | 'Scheduled'
  | 'Paid'
  | 'PartiallyPaid'
  | 'Cancelled';

export type PaymentStatus = 'Pending' | 'Completed' | 'Failed' | 'Cancelled';

export type PaymentMethod =
  | 'BankTransfer'
  | 'Cheque'
  | 'Cash'
  | 'OnlineTransfer';

export type ComplianceType = 'Financial' | 'Operational';
export type ComplianceResult = 'Completed' | 'Flagged';

export type NotificationStatus = 'Unread' | 'Read';

export type ReportStatus = 'Draft' | 'Submitted' | 'Returned' | 'Verified';
