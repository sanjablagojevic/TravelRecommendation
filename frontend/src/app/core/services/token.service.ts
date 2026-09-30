import { Injectable } from '@angular/core';

const TOKEN_KEY = 'travel_recommendation_token';
const USER_KEY = 'travel_recommendation_user';
const EXPIRES_AT_KEY = 'travel_recommendation_expires_at';

/**
 * Persists the JWT in sessionStorage for this browser tab.
 * For production, HttpOnly secure cookies are often safer than client-side token storage
 * because they are not readable by JavaScript and can reduce XSS token theft risk.
 */
@Injectable({ providedIn: 'root' })
export class TokenService {
  getToken(): string | null {
    return sessionStorage.getItem(TOKEN_KEY);
  }

  setToken(token: string): void {
    sessionStorage.setItem(TOKEN_KEY, token);
  }

  removeToken(): void {
    sessionStorage.removeItem(TOKEN_KEY);
  }

  hasToken(): boolean {
    return !!this.getToken();
  }

  getStoredUserJson(): string | null {
    return sessionStorage.getItem(USER_KEY);
  }

  setStoredUserJson(json: string): void {
    sessionStorage.setItem(USER_KEY, json);
  }

  removeStoredUser(): void {
    sessionStorage.removeItem(USER_KEY);
  }

  getExpiresAt(): string | null {
    return sessionStorage.getItem(EXPIRES_AT_KEY);
  }

  setExpiresAt(expiresAt: string): void {
    sessionStorage.setItem(EXPIRES_AT_KEY, expiresAt);
  }

  removeExpiresAt(): void {
    sessionStorage.removeItem(EXPIRES_AT_KEY);
  }

  clearAll(): void {
    this.removeToken();
    this.removeStoredUser();
    this.removeExpiresAt();
  }
}
