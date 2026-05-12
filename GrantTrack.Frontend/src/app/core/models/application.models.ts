// Mirrors GrantTrack/Dto/ApplicationDtos/*.
import { ApplicationStatus } from './enums';

export interface CreateApplicationRequest {
  programId: number;
}

export interface ApplicationResponse {
  applicationId: number;
  programId: number;
  applicantId: number;
  status: ApplicationStatus | string;
  submittedDate: string | null;
}
