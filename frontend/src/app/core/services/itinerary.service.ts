import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/common.models';
import {
  GenerateItineraryRequest,
  ItineraryListItem,
  TravelItinerary,
} from '../models/itinerary.models';

@Injectable({ providedIn: 'root' })
export class ItineraryService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/itineraries`;

  generate(request: GenerateItineraryRequest): Observable<TravelItinerary> {
    return this.http.post<TravelItinerary>(`${this.baseUrl}/generate`, request);
  }

  getList(page = 1, pageSize = 10): Observable<PagedResult<ItineraryListItem>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<ItineraryListItem>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<TravelItinerary> {
    return this.http.get<TravelItinerary>(`${this.baseUrl}/${id}`);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
