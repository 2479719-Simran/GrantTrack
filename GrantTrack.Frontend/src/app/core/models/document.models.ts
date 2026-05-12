// Mirrors GrantTrack/Dto/DocumentDtos/DocumentResponseDto.cs.

export interface AppDocument {
  documentId: number;
  applicationId: number;
  docType: string;
  fileName: string;
  /** Relative URL to GET the file (auth required). */
  downloadUrl: string;
}

/** Doc type categories the FE offers — purely a label, BE accepts any string. */
export const DOC_TYPES: ReadonlyArray<string> = [
  'Project Proposal',
  'Budget Plan',
  'ID Proof',
  'Address Proof',
  'Bank Statement',
  'Tax Document',
  'Other',
];
