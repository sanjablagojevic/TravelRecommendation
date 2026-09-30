import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/common.models';
import {
  AiExplanationsResponse,
  GenerateRecommendationRequest,
  Recommendation,
  RecommendationHistoryItem,
} from '../models/recommendation.models';

@Injectable({ providedIn: 'root' })
export class RecommendationService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/recommendations`;

  generateRecommendation(
    userPreferenceId: number,
    topCount = 5
  ): Observable<Recommendation> {
    const body: GenerateRecommendationRequest = { userPreferenceId, topCount };
    return this.http.post<Recommendation>(this.baseUrl, body);
  }

  getRecommendation(id: number): Observable<Recommendation> {
    return this.http.get<Recommendation>(`${this.baseUrl}/${id}`);
  }

  getHistory(page = 1, pageSize = 10): Observable<PagedResult<RecommendationHistoryItem>> {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    return this.http.get<PagedResult<RecommendationHistoryItem>>(this.baseUrl, { params });
  }

  generateAiExplanations(id: number): Observable<AiExplanationsResponse> {
    return this.http.post<AiExplanationsResponse>(`${this.baseUrl}/${id}/ai-explanations`, {});
  }
}
