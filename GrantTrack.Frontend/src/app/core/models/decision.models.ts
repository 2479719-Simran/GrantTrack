// Mirrors GrantTrack/Dto/DecisionDtos/DecisionDto.
import { DecisionStatus } from './enums';

export interface DecisionRequest {
  applicationId: number;
  approverId: number;
  decisionValue: DecisionStatus;
  notes: string;        // <= 1000 chars (StringLength on backend)
  date: string;         // ISO date string
}
