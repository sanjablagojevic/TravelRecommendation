import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateInterestRequest,
  Interest,
  UpdateInterestRequest,
} from '../models/category.models';

@Injectable({ providedIn: 'root' })
export class InterestService {
  private readonly http = inject(HttpClient);

  getAll(): Observable<Interest[]> {
    return this.http.get<Interest[]>(`${environment.apiUrl}/interests`);
  }

  getById(id: number): Observable<Interest> {
    return this.http.get<Interest>(`${environment.apiUrl}/interests/${id}`);
  }

  create(request: CreateInterestRequest): Observable<Interest> {
    return this.http.post<Interest>(`${environment.apiUrl}/interests`, request);
  }

  update(id: number, request: UpdateInterestRequest): Observable<Interest> {
    return this.http.put<Interest>(`${environment.apiUrl}/interests/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/interests/${id}`);
  }
}
