// Mirrors GrantTrack/Dto/ProgramDtos/* — for Admin program CRUD.

export interface CreateProgramRequest {
  name: string;
  description: string;
  startDate: string;   // ISO yyyy-MM-dd or full ISO; serialized via System.Text.Json
  endDate: string;
  budget: number;
  status: boolean;
}

export interface CreateProgramResponse {
  name: string;
  description: string;
  startDate: string;
  endDate: string;
  budget: number;
  status: boolean;
}

export interface GetProgram {
  programId: number;
  name: string;
  description: string;
  startDate: string;
  endDate: string;
  budget: number;
  status: boolean;
}

export interface UpdateProgramRequest {
  name: string;
  description: string;
  startDate: string;
  endDate: string;
  budget: number;
  status: boolean;
}

export interface UpdateProgramResponse extends GetProgram {}
export interface DeleteProgramResponse extends GetProgram {}

/**
 * Filter shape sent to POST /api/v1/program/FilterPrograms.
 * Backend expects status as a string (`"true"`/`"false"`) per FilterProgramsDto.
 */
export interface FilterProgramsRequest {
  status?: string | null;
  startDate?: string | null;
  endDate?: string | null;
}
