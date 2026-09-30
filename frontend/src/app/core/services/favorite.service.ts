import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Favorite, FavoriteStatus } from '../models/favorite.models';

@Injectable({ providedIn: 'root' })
export class FavoriteService {
  private readonly http = inject(HttpClient);

  getAll(): Observable<Favorite[]> {
    return this.http.get<Favorite[]>(`${environment.apiUrl}/favorites`);
  }

  getStatus(destinationId: number): Observable<FavoriteStatus> {
    return this.http.get<FavoriteStatus>(
      `${environment.apiUrl}/favorites/${destinationId}/status`
    );
  }

  add(destinationId: number): Observable<Favorite> {
    return this.http.post<Favorite>(`${environment.apiUrl}/favorites/${destinationId}`, null);
  }

  remove(destinationId: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/favorites/${destinationId}`);
  }
}
