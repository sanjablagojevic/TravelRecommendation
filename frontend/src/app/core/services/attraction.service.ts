import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Attraction,
  CreateAttractionRequest,
  UpdateAttractionRequest,
} from '../models/destination.models';

@Injectable({ providedIn: 'root' })
export class AttractionService {
  private readonly http = inject(HttpClient);

  getByDestination(destinationId: number): Observable<Attraction[]> {
    return this.http.get<Attraction[]>(
      `${environment.apiUrl}/destinations/${destinationId}/attractions`
    );
  }

  getById(id: number): Observable<Attraction> {
    return this.http.get<Attraction>(`${environment.apiUrl}/attractions/${id}`);
  }

  create(request: CreateAttractionRequest): Observable<Attraction> {
    return this.http.post<Attraction>(`${environment.apiUrl}/attractions`, request);
  }

  update(id: number, request: UpdateAttractionRequest): Observable<Attraction> {
    return this.http.put<Attraction>(`${environment.apiUrl}/attractions/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/attractions/${id}`);
  }
}
