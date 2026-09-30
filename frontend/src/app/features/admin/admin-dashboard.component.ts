import { Component, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { AdminDashboard } from '../../core/models/admin.models';
import { AdminDashboardService } from '../../core/services/admin-dashboard.service';
import { extractApiErrorMessage } from '../../core/utils/api-error.util';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

interface DashboardCard {
  title: string;
  icon: string;
  primary: number;
  primaryLabel: string;
  secondary?: string;
  link?: string;
  linkLabel?: string;
}

@Component({
  selector: 'app-admin-dashboard',
  imports: [MatCardModule, MatIconModule, MatButtonModule, RouterLink, LoadingSpinnerComponent],
  template: `
    <header class="page-header">
      <h1>Admin dashboard</h1>
      <p>Manage catalog content for the travel recommendation platform.</p>
    </header>

    @if (loading()) {
      <app-loading-spinner message="Loading dashboard..." />
    } @else if (error()) {
      <mat-card appearance="outlined" class="error-card">
        <mat-card-content>
          <p class="error">{{ error() }}</p>
          <button mat-flat-button type="button" (click)="load()">Try again</button>
        </mat-card-content>
      </mat-card>
    } @else {
      <div class="cards">
        @for (card of cards(); track card.title) {
          <mat-card class="admin-card" appearance="outlined">
            <mat-card-header>
              <mat-icon mat-card-avatar>{{ card.icon }}</mat-icon>
              <mat-card-title>{{ card.title }}</mat-card-title>
              <mat-card-subtitle>{{ card.primaryLabel }}</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <p class="count">{{ card.primary }}</p>
              @if (card.secondary) {
                <p class="secondary">{{ card.secondary }}</p>
              }
            </mat-card-content>
            @if (card.link) {
              <mat-card-actions align="end">
                <a mat-button color="primary" [routerLink]="card.link">
                  {{ card.linkLabel }}
                </a>
              </mat-card-actions>
            }
          </mat-card>
        }
      </div>
    }
  `,
  styles: `
    :host {
      display: block;
      width: min(1100px, 100%);
      margin: 0 auto;
      padding: 1.5rem 1rem 2rem;
      box-sizing: border-box;
    }

    .page-header h1 {
      font-family: Fraunces, Georgia, serif;
      margin: 0 0 0.5rem;
      color: #0d4f52;
    }

    .page-header p {
      margin: 0 0 1.5rem;
      color: var(--mat-sys-on-surface-variant);
    }

    .cards {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
      gap: 1rem;
    }

    .admin-card {
      display: flex;
      flex-direction: column;
    }

    .admin-card mat-card-content {
      flex: 1;
    }

    .admin-card mat-icon[mat-card-avatar] {
      background: #e0f7fa;
      color: #00796b;
      padding: 0.5rem;
      border-radius: 50%;
    }

    .count {
      margin: 0;
      font-family: Fraunces, Georgia, serif;
      font-size: 2rem;
      font-weight: 600;
      color: #0d4f52;
      line-height: 1.1;
    }

    .secondary {
      margin: 0.25rem 0 0;
      font-size: 0.875rem;
      color: var(--mat-sys-on-surface-variant);
    }

    .error {
      color: var(--mat-sys-error);
      margin: 0 0 1rem;
    }

    .error-card {
      max-width: 480px;
    }
  `,
})
export class AdminDashboardComponent {
  private readonly dashboardService = inject(AdminDashboardService);

  readonly dashboard = signal<AdminDashboard | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  readonly cards = computed<DashboardCard[]>(() => {
    const data = this.dashboard();
    if (!data) {
      return [];
    }

    return [
      {
        title: 'Destinations',
        icon: 'public',
        primary: data.destinationsCount,
        primaryLabel: 'Total destinations',
        secondary: `${data.activeDestinationsCount} active · ${data.inactiveDestinationsCount} inactive`,
        link: '/admin/destinations',
        linkLabel: 'Manage destinations',
      },
      {
        title: 'Categories',
        icon: 'category',
        primary: data.categoriesCount,
        primaryLabel: 'Travel categories',
        link: '/admin/categories',
        linkLabel: 'Manage categories',
      },
      {
        title: 'Interests',
        icon: 'interests',
        primary: data.interestsCount,
        primaryLabel: 'Interest tags',
        link: '/admin/interests',
        linkLabel: 'Manage interests',
      },
      {
        title: 'Attractions',
        icon: 'place',
        primary: data.attractionsCount,
        primaryLabel: 'Points of interest',
        secondary: 'Edit a destination to manage its attractions.',
        link: '/admin/destinations',
        linkLabel: 'Open destinations',
      },
      {
        title: 'Activities',
        icon: 'directions_run',
        primary: data.activitiesCount,
        primaryLabel: 'Suggested activities',
        secondary: 'Edit a destination to manage its activities.',
        link: '/admin/destinations',
        linkLabel: 'Open destinations',
      },
      {
        title: 'Users',
        icon: 'group',
        primary: data.usersCount,
        primaryLabel: 'Registered users',
      },
      {
        title: 'Recommendations',
        icon: 'insights',
        primary: data.recommendationsCount,
        primaryLabel: 'Generated recommendations',
      },
    ];
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.dashboardService.getDashboard().subscribe({
      next: (data) => {
        this.dashboard.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(extractApiErrorMessage(err, 'Unable to load the dashboard.'));
        this.loading.set(false);
      },
    });
  }
}
