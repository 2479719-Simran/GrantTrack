// Auth-related DTOs — see GrantTrack/Dto/LoginDtos and GrantTrack/Dto/UserDTOs.
import { UserRole } from './enums';

export interface LoginRequest {
  email: string;
  password: string;
}

/**
 * The login endpoint (POST /api/v1/user/login) returns the raw JWT string in
 * the success body — NOT an envelope object. See UserController.Login.
 */
export type LoginResponse = string;

/**
 * Decoded JWT claims issued by UserService.GenerateJwtTokenServiceAsync.
 * Note that property names are the standard JWT claim names because the
 * backend uses JwtRegisteredClaimNames + ClaimTypes.Role.
 */
export interface JwtClaims {
  sub: string;            // user id (as string)
  email: string;
  role: UserRole;         // serialized via ClaimTypes.Role
  jti: string;
  exp: number;            // unix seconds
  iat?: number;
  aud?: string;
  iss?: string;
}

export interface CurrentUser {
  userId: number;
  email: string;
  role: UserRole;
  exp: number;            // unix seconds — for client-side expiry checks
}

export interface RegisterUserRequest {
  name: string;
  email: string;
  password: string;
  phone: string;          // exactly 10 digits — RegisterUserDto regex
  role?: UserRole | null; // backend defaults to Applicant when omitted
}

export interface ForgotPasswordRequest {
  email: string;
  newPassword: string;
  confirmPassword: string;
}
