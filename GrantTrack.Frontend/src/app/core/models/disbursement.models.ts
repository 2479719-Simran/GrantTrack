// Mirrors GrantTrack/Dto/DisbursementDtos/*.
import { DisbursementStatus, PaymentMethod } from './enums';

export interface CreateDisbursementRequest {
  applicationId: number;
  amount: number;          // > 0
  scheduledDate: string;   // ISO
}

export interface UpdateDisbursementRequest {
  amount?: number | null;
  scheduledDate?: string | null;
  actualDate?: string | null;
  status?: DisbursementStatus | null;
}

export interface DisbursementResponse {
  disbursementId: number;
  applicationId: number;
  amount: number;
  scheduledDate: string;
  actualDate: string | null;
  status: string;          // backend returns enum name as string
}

export interface CreatePaymentRequest {
  disbursementId: number;
  amount: number;          // > 0
  date: string;            // ISO
  method: PaymentMethod;
}

export interface PaymentResponse {
  paymentId: number;
  disbursementId: number;
  amount: number;
  date: string;
  method: string;
  status: string;
}
