import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RouterLink } from '@angular/router';
import { FavoriteService } from '../../../core/services/favorite.service';
import { Favorite } from '../../../core/models/favorite.models';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { extractApiErrorMessage } from '../../../core/utils/api-error.util';

@Component({
  selector: 'app-favorites-page',
  imports: [
    CurrencyPipe,
    DatePipe,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    LoadingSpinnerComponent,
  ],
  template: `
    <header class="page-header">
      <h1>My favorites</h1>
      <p>Saved destinations you want to revisit.</p>
    </header>

    @if (loading()) {
      <app-loading-spinner message="Loading favorites..." />
    } @else if (error()) {
      <p class="error">{{ error() }}</p>
    } @else if (!favorites().length) {
      <mat-card>
        <mat-card-content>
          <p>You haven't saved any destinations yet.</p>
          <a mat-flat-button color="primary" routerLink="/destinations">Explore destinations</a>
        </mat-card-content>
      </mat-card>
    } @else {
      <div class="list">
        @for (favorite of favorites(); track favorite.id) {
          <mat-card>
            <mat-card-header>
              <mat-card-title>{{ favorite.destination.name }}</mat-card-title>
              <mat-card-subtitle>
                {{ favorite.destination.city }}, {{ favorite.destination.country }}
              </mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <p>
                {{ favorite.destination.averageDailyCost | currency: 'USD' : 'symbol' : '1.0-0' }}/day
                · Saved {{ favorite.addedAt | date: 'mediumDate' }}
              </p>
            </mat-card-content>
            <mat-card-actions align="end">
              <button mat-button color="warn" type="button" (click)="remove(favorite)">
                <mat-icon>delete</mat-icon>
                Remove
              </button>
              <a mat-button color="primary" [routerLink]="['/destinations', favorite.destination.id]">
                View
              </a>
            </mat-card-actions>
          </mat-card>
        }
      </div>
    }
  `,
  styles: `
    .page-header h1 {
      margin: 0 0 0.25rem;
    }

    .page-header p {
      margin: 0 0 1.5rem;
      color: var(--mat-sys-on-surface-variant);
    }

    .list {
      display: grid;
      gap: 1rem;
    }

    .error {
      color: var(--mat-sys-error);
    }

    :host {
      display: block;
      width: min(1100px, 100%);
      margin: 0 auto;
      padding: 1.5rem 1rem 2rem;
      box-sizing: border-box;
    }
  `,
})
export class FavoritesPageComponent {
  private readonly favoriteService = inject(FavoriteService);
  private readonly snackBar = inject(MatSnackBar);

  readonly favorites = signal<Favorite[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  constructor() {
    this.load();
  }

  remove(favorite: Favorite): void {
    this.favoriteService.remove(favorite.destination.id).subscribe({
      next: () => {
        this.favorites.update((items) => items.filter((item) => item.id !== favorite.id));
        this.snackBar.open('Removed from favorites', 'Dismiss', { duration: 3000 });
      },
      error: (err) => {
        this.snackBar.open(extractApiErrorMessage(err), 'Dismiss', { duration: 5000 });
      },
    });
  }

  private load(): void {
    this.loading.set(true);
    this.favoriteService.getAll().subscribe({
      next: (items) => {
        this.favorites.set(items);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(extractApiErrorMessage(err));
        this.loading.set(false);
      },
    });
  }
}
