import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RouterLink } from '@angular/router';
import { ItineraryListItem } from '../../core/models/itinerary.models';
import { ItineraryService } from '../../core/services/itinerary.service';
import { extractApiErrorMessage } from '../../core/utils/api-error.util';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-itinerary-list',
  imports: [
    DatePipe,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatPaginatorModule,
    LoadingSpinnerComponent,
  ],
  template: `
    <header class="page-header">
      <h1>My trips</h1>
      <p>Day-by-day travel plans you have generated for your saved destinations.</p>
    </header>

    @if (loading()) {
      <app-loading-spinner message="Loading your itineraries..." />
    } @else if (error()) {
      <p class="error">{{ error() }}</p>
    } @else if (!items().length) {
      <mat-card class="empty-card">
        <mat-card-content>
          <mat-icon class="empty-icon">map</mat-icon>
          <p>You have not created any itineraries yet.</p>
          <a mat-flat-button color="primary" routerLink="/plan-trip">Plan a trip</a>
        </mat-card-content>
      </mat-card>
    } @else {
      <div class="list">
        @for (itinerary of items(); track itinerary.id) {
          <mat-card class="itinerary-card">
            <div class="card-layout">
              <div class="image-wrap">
                @if (itinerary.destinationImageUrl) {
                  <img
                    [src]="itinerary.destinationImageUrl"
                    [alt]="itinerary.destinationName"
                    loading="lazy"
                    (error)="onImageError($event)"
                  />
                } @else {
                  <div class="image-fallback">
                    <mat-icon>landscape</mat-icon>
                  </div>
                }
              </div>

              <div class="card-body">
                <h2>{{ itinerary.title }}</h2>
                <p class="destination">{{ itinerary.destinationName }}</p>
                <p class="meta">
                  {{ itinerary.durationDays }} day{{ itinerary.durationDays === 1 ? '' : 's' }}
                  · Created {{ itinerary.createdAt | date: 'mediumDate' }}
                </p>

                <div class="actions">
                  <a mat-flat-button color="primary" [routerLink]="['/itineraries', itinerary.id]">
                    View
                  </a>
                  <button
                    mat-button
                    type="button"
                    class="delete-btn"
                    [disabled]="deletingId() === itinerary.id"
                    (click)="confirmDelete(itinerary)"
                  >
                    Delete
                  </button>
                </div>
              </div>
            </div>
          </mat-card>
        }
      </div>

      <mat-paginator
        [length]="totalCount()"
        [pageIndex]="page() - 1"
        [pageSize]="pageSize()"
        [pageSizeOptions]="[5, 10, 20]"
        (page)="onPage($event)"
        aria-label="Itinerary pages"
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

    .itinerary-card {
      overflow: hidden;
    }

    .card-layout {
      display: grid;
      grid-template-columns: minmax(140px, 200px) 1fr;
      gap: 1rem;
    }

    @media (max-width: 640px) {
      .card-layout {
        grid-template-columns: 1fr;
      }
    }

    .image-wrap img {
      width: 100%;
      height: 100%;
      min-height: 130px;
      object-fit: cover;
      border-radius: 8px;
    }

    .image-fallback {
      min-height: 130px;
      border-radius: 8px;
      background: linear-gradient(145deg, #0d7377 0%, #2a9d8f 100%);
      display: flex;
      align-items: center;
      justify-content: center;
      color: rgba(255, 255, 255, 0.85);
    }

    .image-fallback mat-icon {
      font-size: 2.5rem;
      width: 2.5rem;
      height: 2.5rem;
    }

    h2 {
      margin: 0;
      font-family: 'Fraunces', Georgia, serif;
      font-size: 1.25rem;
    }

    .destination {
      margin: 0.2rem 0 0;
      font-weight: 500;
    }

    .meta {
      margin: 0.2rem 0 0.75rem;
      color: var(--mat-sys-on-surface-variant);
      font-size: 0.9rem;
    }

    .actions {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
    }

    .delete-btn {
      color: var(--mat-sys-error);
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
export class ItineraryListComponent {
  private readonly itineraryService = inject(ItineraryService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly placeholder = 'assets/images/destination-placeholder.svg';
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly items = signal<ItineraryListItem[]>([]);
  readonly page = signal(1);
  readonly pageSize = signal(10);
  readonly totalCount = signal(0);
  readonly deletingId = signal<number | null>(null);

  constructor() {
    this.loadPage(1, 10);
  }

  onPage(event: PageEvent): void {
    this.loadPage(event.pageIndex + 1, event.pageSize);
  }

  onImageError(event: Event): void {
    (event.target as HTMLImageElement).src = this.placeholder;
  }

  confirmDelete(itinerary: ItineraryListItem): void {
    const data: ConfirmDialogData = {
      title: 'Delete itinerary',
      message: `Delete "${itinerary.title}"? This cannot be undone.`,
      confirmText: 'Delete',
      confirmColor: 'warn',
    };

    this.dialog
      .open<ConfirmDialogComponent, ConfirmDialogData, boolean>(ConfirmDialogComponent, { data })
      .afterClosed()
      .subscribe((confirmed) => {
        if (confirmed) {
          this.deleteItinerary(itinerary.id);
        }
      });
  }

  private deleteItinerary(id: number): void {
    this.deletingId.set(id);
    this.itineraryService.delete(id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.snackBar.open('Itinerary deleted.', 'Dismiss', { duration: 3000 });
        const remaining = this.items().length - 1;
        const targetPage = remaining === 0 && this.page() > 1 ? this.page() - 1 : this.page();
        this.loadPage(targetPage, this.pageSize());
      },
      error: (err) => {
        this.deletingId.set(null);
        this.snackBar.open(
          extractApiErrorMessage(err, 'Could not delete this itinerary.'),
          'Dismiss',
          { duration: 5000 }
        );
      },
    });
  }

  private loadPage(page: number, pageSize: number): void {
    this.loading.set(true);
    this.error.set(null);
    this.page.set(page);
    this.pageSize.set(pageSize);

    this.itineraryService.getList(page, pageSize).subscribe({
      next: (result) => {
        this.items.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(extractApiErrorMessage(err, 'Could not load your itineraries.'));
        this.loading.set(false);
      },
    });
  }
}
