// Mirrors GrantTrack/Dto/FinanceDtos/*.

export interface FinanceApplicationListRow {
  applicationId: number;
  applicantId: number;
  applicantName: string;
  applicantEmail: string;
  programId: number;
  programName: string;
  programBudget: number;
  decisionDate: string;

  disbursementCount: number;
  totalDisbursed: number;
  totalPaid: number;
  remaining: number;

  pendingCount: number;
  scheduledCount: number;
  paidCount: number;
  partiallyPaidCount: number;
  cancelledCount: number;
}

export interface FinancePaymentInfo {
  paymentId: number;
  amount: number;
  date: string;
  method: string;       // "BankTransfer" | "Cheque" | "Cash" | "OnlineTransfer"
  status: string;       // "Pending" | "Completed" | "Failed" | "Cancelled"
}

export interface FinanceDisbursementInfo {
  disbursementId: number;
  amount: number;
  scheduledDate: string;
  actualDate: string | null;
  status: string;       // "Pending" | "Scheduled" | "Paid" | "PartiallyPaid" | "Cancelled"
  payments: FinancePaymentInfo[];
  totalPaid: number;
  remainingOnTranche: number;
}

export interface FinanceApplicationDetail {
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

  decisionId: number | null;
  approverId: number | null;
  approverName: string | null;
  decisionValue: string | null;
  decisionNotes: string | null;
  decisionDate: string | null;

  disbursements: FinanceDisbursementInfo[];
  totalDisbursed: number;
  totalPaid: number;
  remaining: number;
}
