import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AiPreferenceExtraction,
  AiPreferenceRequest,
  CreatePreferenceFromAiRequest,
  CreatePreferenceFromAiResponse,
} from '../models/ai.models';

@Injectable({ providedIn: 'root' })
export class AiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/ai`;

  /** Calls ASP.NET only — never OpenAI directly from Angular. */
  extractPreferences(message: string): Observable<AiPreferenceExtraction> {
    const body: AiPreferenceRequest = { message };
    return this.http.post<AiPreferenceExtraction>(`${this.baseUrl}/extract-preferences`, body);
  }

  createPreferenceFromConfirmedData(
    request: CreatePreferenceFromAiRequest
  ): Observable<CreatePreferenceFromAiResponse> {
    return this.http.post<CreatePreferenceFromAiResponse>(
      `${this.baseUrl}/create-preference`,
      request
    );
  }
}
