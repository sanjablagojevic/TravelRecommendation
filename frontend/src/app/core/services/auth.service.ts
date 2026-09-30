import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, finalize, map, of, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AuthResponse,
  CurrentUser,
  LoginRequest,
  RegisterRequest,
  StoredAuth,
} from '../models/auth.models';
import { TokenService } from './token.service';
import { TripPlanningStateService } from './trip-planning-state.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly tokenService = inject(TokenService);
  private readonly tripPlanningState = inject(TripPlanningStateService);

  private readonly authState = signal<StoredAuth | null>(this.readStoredAuth());

  readonly currentUser = computed(() => this.authState()?.user ?? null);
  readonly isAuthenticated = computed(() => {
    const state = this.authState();
    if (!state?.token) {
      return false;
    }
    const expires = DateParseSafe(state.expiresAt);
    return expires != null && expires > Date.now();
  });

  readonly initializing = signal(false);

  getToken(): string | null {
    return this.isAuthenticated() ? this.authState()?.token ?? null : null;
  }

  getCurrentUser(): CurrentUser | null {
    return this.currentUser();
  }

  isAdmin(): boolean {
    return this.getCurrentUser()?.role === 'Admin';
  }

  initialize(): Observable<CurrentUser | null> {
    if (!this.isAuthenticated()) {
      this.clearSession();
      return of(null);
    }

    this.initializing.set(true);
    return this.http.get<CurrentUser>(`${environment.apiUrl}/auth/me`).pipe(
      tap((user) => {
        const current = this.authState();
        if (current) {
          this.persist({ ...current, user });
        }
      }),
      catchError(() => {
        this.clearSession();
        return of(null);
      }),
      finalize(() => this.initializing.set(false))
    );
  }

  login(request: LoginRequest): Observable<CurrentUser> {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/login`, request)
      .pipe(map((response) => this.applyAuthResponse(response)));
  }

  register(request: RegisterRequest): Observable<CurrentUser> {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/register`, request)
      .pipe(map((response) => this.applyAuthResponse(response)));
  }

  logout(redirect = true): void {
    this.clearSession();
    if (redirect) {
      void this.router.navigate(['/']);
    }
  }

  clearSession(): void {
    this.authState.set(null);
    this.tokenService.clearAll();
    this.tripPlanningState.clearAll();
  }

  private applyAuthResponse(response: AuthResponse): CurrentUser {
    // New login/register replaces any previous user's in-memory planning state.
    this.tripPlanningState.clearAll();
    this.persist({
      token: response.token,
      expiresAt: response.expiresAt,
      user: response.user,
    });
    return response.user;
  }

  private persist(state: StoredAuth): void {
    this.authState.set(state);
    this.tokenService.setToken(state.token);
    this.tokenService.setExpiresAt(state.expiresAt);
    this.tokenService.setStoredUserJson(JSON.stringify(state.user));
  }

  private readStoredAuth(): StoredAuth | null {
    try {
      if (!this.tokenService.hasToken()) {
        return null;
      }
      const token = this.tokenService.getToken();
      const expiresAt = this.tokenService.getExpiresAt();
      const userRaw = this.tokenService.getStoredUserJson();
      if (!token || !expiresAt || !userRaw) {
        this.tokenService.clearAll();
        return null;
      }
      const expires = DateParseSafe(expiresAt);
      if (expires == null || expires <= Date.now()) {
        this.tokenService.clearAll();
        return null;
      }
      const user = JSON.parse(userRaw) as CurrentUser;
      return { token, expiresAt, user };
    } catch {
      this.tokenService.clearAll();
      return null;
    }
  }
}

function DateParseSafe(value: string): number | null {
  const expires = Date.parse(value);
  return Number.isNaN(expires) ? null : expires;
}
