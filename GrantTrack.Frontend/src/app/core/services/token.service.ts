import { Injectable } from '@angular/core';
import { jwtDecode } from 'jwt-decode';
import { CurrentUser } from '../models/auth.models';
import { UserRole } from '../models/enums';

/**
 * Single source of truth for the JWT bearer token. Keeps token I/O isolated
 * so the auth service and HTTP interceptor don't both touch localStorage.
 *
 * Storage key is intentionally short and namespaced. We persist to
 * localStorage so the session survives page reloads — a refresh-token flow
 * can be layered on later without changing this contract.
 */
@Injectable({ providedIn: 'root' })
export class TokenService {
  private static readonly KEY = 'gt.token';

  /**
   * Possible JWT claim names .NET can use for ClaimTypes.Role / NameIdentifier
   * depending on the JwtSecurityTokenHandler outbound-claim-map configuration.
   * We probe each in priority order — first hit wins.
   */
  private static readonly ROLE_CLAIM_KEYS = [
    'role',
    'roles',
    'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
  ];
  private static readonly USER_ID_CLAIM_KEYS = [
    'sub',
    'nameid',
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
  ];
  private static readonly EMAIL_CLAIM_KEYS = [
    'email',
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress',
  ];

  getToken(): string | null {
    return localStorage.getItem(TokenService.KEY);
  }

  setToken(token: string): void {
    localStorage.setItem(TokenService.KEY, token);
  }

  clear(): void {
    localStorage.removeItem(TokenService.KEY);
  }

  /**
   * Decodes the current token. Returns null when missing or unparseable.
   * Robust against the various claim-name shapes .NET can emit.
   */
  decode(): CurrentUser | null {
    const token = this.getToken();
    if (!token) return null;

    let raw: Record<string, unknown>;
    try {
      raw = jwtDecode<Record<string, unknown>>(token);
    } catch {
      return null;
    }

    const userId = Number(this.firstClaim(raw, TokenService.USER_ID_CLAIM_KEYS));
    if (!Number.isFinite(userId)) return null;

    const email = String(this.firstClaim(raw, TokenService.EMAIL_CLAIM_KEYS) ?? '');
    const roleRaw = this.firstClaim(raw, TokenService.ROLE_CLAIM_KEYS);
    // Some IdPs ship `role` as an array when the user has multiple roles —
    // GrantTrack issues exactly one, but defend anyway.
    const role = (Array.isArray(roleRaw) ? roleRaw[0] : roleRaw) as UserRole;
    const exp = Number(raw['exp']);

    if (!role) {
      // One-time dev-mode warning so the user can spot a misconfigured token
      // (e.g. backend forgot to add the role claim) without us silently
      // pretending everything is fine.
      // eslint-disable-next-line no-console
      console.warn(
        '[TokenService] JWT decoded but no role claim was found. Looked at:',
        TokenService.ROLE_CLAIM_KEYS,
        'Token payload keys:', Object.keys(raw),
      );
    }

    return { userId, email, role, exp };
  }

  isExpired(now: number = Math.floor(Date.now() / 1000)): boolean {
    const user = this.decode();
    if (!user) return true;
    return user.exp <= now;
  }

  /**
   * Helper: returns the value of the first key in `keys` that's present in
   * `claims` and not null/empty. Used to probe alternative claim names.
   */
  private firstClaim(
    claims: Record<string, unknown>,
    keys: readonly string[],
  ): unknown {
    for (const k of keys) {
      const v = claims[k];
      if (v !== undefined && v !== null && v !== '') return v;
    }
    return undefined;
  }
}
