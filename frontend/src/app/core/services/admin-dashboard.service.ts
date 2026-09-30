import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AdminDashboard } from '../models/admin.models';

@Injectable({ providedIn: 'root' })
export class AdminDashboardService {
  private readonly http = inject(HttpClient);

  getDashboard(): Observable<AdminDashboard> {
    return this.http.get<AdminDashboard>(`${environment.apiUrl}/admin/dashboard`);
  }
}
