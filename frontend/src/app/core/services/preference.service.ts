import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateUserPreferenceRequest,
  UserPreference,
} from '../models/preference.models';

@Injectable({ providedIn: 'root' })
export class PreferenceService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/preferences`;

  createPreference(request: CreateUserPreferenceRequest): Observable<UserPreference> {
    return this.http.post<UserPreference>(this.baseUrl, request);
  }

  getPreferences(): Observable<UserPreference[]> {
    return this.http.get<UserPreference[]>(this.baseUrl);
  }

  getPreference(id: number): Observable<UserPreference> {
    return this.http.get<UserPreference>(`${this.baseUrl}/${id}`);
  }

  updatePreference(id: number, request: CreateUserPreferenceRequest): Observable<UserPreference> {
    return this.http.put<UserPreference>(`${this.baseUrl}/${id}`, request);
  }

  deletePreference(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
