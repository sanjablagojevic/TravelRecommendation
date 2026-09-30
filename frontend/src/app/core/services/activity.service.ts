import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Activity,
  CreateActivityRequest,
  UpdateActivityRequest,
} from '../models/destination.models';

@Injectable({ providedIn: 'root' })
export class ActivityService {
  private readonly http = inject(HttpClient);

  getByDestination(destinationId: number): Observable<Activity[]> {
    return this.http.get<Activity[]>(
      `${environment.apiUrl}/destinations/${destinationId}/activities`
    );
  }

  getById(id: number): Observable<Activity> {
    return this.http.get<Activity>(`${environment.apiUrl}/activities/${id}`);
  }

  create(request: CreateActivityRequest): Observable<Activity> {
    return this.http.post<Activity>(`${environment.apiUrl}/activities`, request);
  }

  update(id: number, request: UpdateActivityRequest): Observable<Activity> {
    return this.http.put<Activity>(`${environment.apiUrl}/activities/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/activities/${id}`);
  }
}
