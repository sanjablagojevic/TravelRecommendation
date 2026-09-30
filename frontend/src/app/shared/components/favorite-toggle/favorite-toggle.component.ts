import { Component, effect, inject, input, output, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router, RouterLink } from '@angular/router';
import { FavoriteService } from '../../../core/services/favorite.service';
import { AuthService } from '../../../core/services/auth.service';
import { extractApiErrorMessage } from '../../../core/utils/api-error.util';

@Component({
  selector: 'app-favorite-toggle',
  imports: [MatButtonModule, MatIconModule, RouterLink],
  template: `
    @if (!auth.isAuthenticated()) {
      <a mat-stroked-button routerLink="/login" [queryParams]="{ returnUrl: router.url }">
        Sign in to save
      </a>
    } @else {
      <button
        mat-icon-button
        type="button"
        [attr.aria-label]="isFavorite() ? 'Remove from favorites' : 'Add to favorites'"
        [disabled]="busy()"
        (click)="toggle()"
      >
        <mat-icon>{{ isFavorite() ? 'favorite' : 'favorite_border' }}</mat-icon>
      </button>
    }
  `,
})
export class FavoriteToggleComponent {
  private readonly favorites = inject(FavoriteService);
  readonly auth = inject(AuthService);
  private readonly snackBar = inject(MatSnackBar);
  readonly router = inject(Router);

  readonly destinationId = input.required<number>();
  readonly initialFavorite = input<boolean | null>(null);
  readonly favoriteChange = output<boolean>();

  readonly isFavorite = signal(false);
  readonly busy = signal(false);

  constructor() {
    effect(() => {
      const initial = this.initialFavorite();
      if (initial != null) {
        this.isFavorite.set(initial);
      }
    });
  }

  toggle(): void {
    if (!this.auth.isAuthenticated()) {
      void this.router.navigate(['/login'], {
        queryParams: { returnUrl: this.router.url },
      });
      return;
    }

    this.busy.set(true);
    const id = this.destinationId();

    if (this.isFavorite()) {
      this.favorites.remove(id).subscribe({
        next: () => {
          this.isFavorite.set(false);
          this.favoriteChange.emit(false);
          this.busy.set(false);
          this.snackBar.open('Removed from favorites.', 'OK', { duration: 3000 });
        },
        error: (error: unknown) => {
          this.busy.set(false);
          this.snackBar.open(extractApiErrorMessage(error), 'Dismiss', { duration: 5000 });
        },
      });
      return;
    }

    this.favorites.add(id).subscribe({
      next: () => {
        this.isFavorite.set(true);
        this.favoriteChange.emit(true);
        this.busy.set(false);
        this.snackBar.open('Added to favorites.', 'OK', { duration: 3000 });
      },
      error: (error: unknown) => {
        // Duplicate favorite — sync UI instead of crashing.
        if (error instanceof HttpErrorResponse && error.status === 409) {
          this.isFavorite.set(true);
          this.favoriteChange.emit(true);
          this.busy.set(false);
          this.snackBar.open('Added to favorites.', 'OK', { duration: 3000 });
          return;
        }
        this.busy.set(false);
        this.snackBar.open(extractApiErrorMessage(error), 'Dismiss', { duration: 5000 });
      },
    });
  }
}
