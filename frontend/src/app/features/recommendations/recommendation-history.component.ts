import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { RouterLink } from '@angular/router';
import { RecommendationHistoryItem } from '../../core/models/recommendation.models';
import { RecommendationService } from '../../core/services/recommendation.service';
import { extractApiErrorMessage } from '../../core/utils/api-error.util';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-recommendation-history',
  imports: [
    DatePipe,
    DecimalPipe,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatPaginatorModule,
    LoadingSpinnerComponent,
  ],
  template: `
    <header class="page-header">
      <h1>Recommendation history</h1>
      <p>Past trips you have generated with the recommendation engine.</p>
    </header>

    @if (loading()) {
      <app-loading-spinner message="Loading history..." />
    } @else if (error()) {
      <p class="error">{{ error() }}</p>
    } @else if (!items().length) {
      <mat-card class="empty-card">
        <mat-card-content>
          <mat-icon class="empty-icon">travel_explore</mat-icon>
          <p>You have not generated any recommendations yet.</p>
          <a mat-flat-button color="primary" routerLink="/plan-trip">Plan a trip</a>
        </mat-card-content>
      </mat-card>
    } @else {
      <div class="list">
        @for (entry of items(); track entry.id) {
          <mat-card>
            <mat-card-header>
              <mat-card-title>{{ entry.topDestination || 'Trip recommendation' }}</mat-card-title>
              <mat-card-subtitle>{{ entry.createdAt | date: 'medium' }}</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <p>
                @if (entry.topScore != null) {
                  Top match: <strong>{{ entry.topScore * 100 | number: '1.2-2' }}%</strong>
                  ·
                }
                {{ entry.numberOfResults }} destination{{ entry.numberOfResults === 1 ? '' : 's' }}
              </p>
            </mat-card-content>
            <mat-card-actions align="end">
              <a mat-button color="primary" [routerLink]="['/recommendations', entry.id]">View</a>
            </mat-card-actions>
          </mat-card>
        }
      </div>

      <mat-paginator
        [length]="totalCount()"
        [pageIndex]="page() - 1"
        [pageSize]="pageSize()"
        [pageSizeOptions]="[5, 10, 20]"
        (page)="onPage($event)"
        aria-label="Recommendation history pages"
      />
    }
  `,
  styles: `
    :host {
      display: block;
      width: min(880px, 100%);
      margin: 0 auto;
      padding: 1.5rem 1rem 2.5rem;
      box-sizing: border-box;
    }

    h1 {
      font-family: 'Fraunces', Georgia, serif;
      margin: 0 0 0.35rem;
      color: #0d4f52;
    }

    .page-header p {
      margin: 0 0 1.5rem;
      color: var(--mat-sys-on-surface-variant);
    }

    .list {
      display: flex;
      flex-direction: column;
      gap: 1rem;
      margin-bottom: 1rem;
    }

    .empty-card {
      text-align: center;
      padding: 1.5rem 1rem;
    }

    .empty-icon {
      font-size: 3rem;
      width: 3rem;
      height: 3rem;
      color: #0d7377;
      margin-bottom: 0.5rem;
    }

    .error {
      color: var(--mat-sys-error);
    }
  `,
})
export class RecommendationHistoryComponent {
  private readonly recommendationService = inject(RecommendationService);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly items = signal<RecommendationHistoryItem[]>([]);
  readonly page = signal(1);
  readonly pageSize = signal(10);
  readonly totalCount = signal(0);

  constructor() {
    this.loadPage(1, 10);
  }

  onPage(event: PageEvent): void {
    this.loadPage(event.pageIndex + 1, event.pageSize);
  }

  private loadPage(page: number, pageSize: number): void {
    this.loading.set(true);
    this.error.set(null);
    this.page.set(page);
    this.pageSize.set(pageSize);

    this.recommendationService.getHistory(page, pageSize).subscribe({
      next: (result) => {
        this.items.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(extractApiErrorMessage(err, 'Could not load history.'));
        this.loading.set(false);
      },
    });
  }
}
