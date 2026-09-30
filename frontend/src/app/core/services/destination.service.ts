import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult, DestinationQuery } from '../models/common.models';
import {
  CreateDestinationRequest,
  DestinationDetails,
  DestinationListItem,
  UpdateDestinationRequest,
} from '../models/destination.models';

@Injectable({ providedIn: 'root' })
export class DestinationService {
  private readonly http = inject(HttpClient);

  getDestinations(query: DestinationQuery): Observable<PagedResult<DestinationListItem>> {
    let params = new HttpParams();
    if (query.search) {
      params = params.set('search', query.search);
    }
    if (query.country) {
      params = params.set('country', query.country);
    }
    if (query.city) {
      params = params.set('city', query.city);
    }
    if (query.categoryId != null) {
      params = params.set('categoryId', query.categoryId);
    }
    if (query.interestId != null) {
      params = params.set('interestId', query.interestId);
    }
    if (query.isActive != null) {
      params = params.set('isActive', query.isActive);
    }
    if (query.includeInactive) {
      params = params.set('includeInactive', true);
    }
    params = params.set('page', query.page ?? 1);
    params = params.set('pageSize', query.pageSize ?? 12);

    return this.http.get<PagedResult<DestinationListItem>>(
      `${environment.apiUrl}/destinations`,
      { params }
    );
  }

  getById(id: number): Observable<DestinationDetails> {
    return this.http.get<DestinationDetails>(`${environment.apiUrl}/destinations/${id}`);
  }

  create(request: CreateDestinationRequest): Observable<DestinationDetails> {
    return this.http.post<DestinationDetails>(`${environment.apiUrl}/destinations`, request);
  }

  update(id: number, request: UpdateDestinationRequest): Observable<DestinationDetails> {
    return this.http.put<DestinationDetails>(`${environment.apiUrl}/destinations/${id}`, request);
  }

  /** Soft-deactivates the destination on the backend. */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/destinations/${id}`);
  }
}
