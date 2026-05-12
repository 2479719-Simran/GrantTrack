// Mirrors GrantTrack/Dto/UserDTOs/* — used by the Admin user-management screens.
import { UserRole } from './enums';

export interface ViewUser {
  userId: number;
  name: string | null;
  email: string | null;
  status: boolean;
  role: string | null;
}

/**
 * Sent to POST /api/v1/user/update/{id}.
 * Backend expects role as a string ("Admin", "Reviewer", ...) not the enum number,
 * because UpdateUserRequestDto.Role is typed as string?.
 */
export interface UpdateUserRequest {
  name?: string | null;
  phone?: string | null;
  role?: UserRole | string | null;
  status: boolean;
  email?: string | null;
}

export interface UpdateUserResponse {
  name: string | null;
  role: string | null;
  phone: string | null;
  status: boolean;
  email: string | null;
}
