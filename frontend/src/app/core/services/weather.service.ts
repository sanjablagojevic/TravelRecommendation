import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { WeatherResponse } from '../models/weather.models';

@Injectable({ providedIn: 'root' })
export class WeatherService {
  private readonly http = inject(HttpClient);

  getByDestinationId(destinationId: number): Observable<WeatherResponse> {
    return this.http.get<WeatherResponse>(
      `${environment.apiUrl}/destinations/${destinationId}/weather`
    );
  }
}
