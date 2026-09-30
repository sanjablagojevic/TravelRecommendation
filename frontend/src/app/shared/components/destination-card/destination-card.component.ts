import { CurrencyPipe } from '@angular/common';
import { Component, input, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { RouterLink } from '@angular/router';
import { DestinationListItem } from '../../../core/models/destination.models';

@Component({
  selector: 'app-destination-card',
  imports: [MatCardModule, MatButtonModule, MatChipsModule, RouterLink, CurrencyPipe],
  template: `
    <mat-card class="destination-card">
      <div class="media">
        @if (destination().imageUrl && !imageFailed()) {
          <img
            [src]="destination().imageUrl!"
            [alt]="destination().name"
            loading="lazy"
            (error)="onImageError()"
          />
        } @else {
          <img
            class="fallback"
            src="/assets/images/destination-placeholder.svg"
            [alt]="destination().name + ' placeholder'"
            loading="lazy"
          />
        }
      </div>
      <mat-card-header>
        <mat-card-title>{{ destination().name }}</mat-card-title>
        <mat-card-subtitle>{{ destination().city }}, {{ destination().country }}</mat-card-subtitle>
      </mat-card-header>
      <mat-card-content>
        <p class="meta">
          <span>{{ destination().averageDailyCost | currency: 'USD' : 'symbol' : '1.0-0' }}/day</span>
          @if (destination().climate) {
            <span>{{ destination().climate }}</span>
          }
        </p>
        @if (destination().categories.length) {
          <mat-chip-set aria-label="Categories">
            @for (category of destination().categories.slice(0, 3); track category.id) {
              <mat-chip>{{ category.name }}</mat-chip>
            }
          </mat-chip-set>
        }
      </mat-card-content>
      <mat-card-actions align="end">
        <a mat-button color="primary" [routerLink]="['/destinations', destination().id]">View details</a>
      </mat-card-actions>
    </mat-card>
  `,
  styles: `
    .destination-card {
      height: 100%;
      display: flex;
      flex-direction: column;
    }

    .media {
      aspect-ratio: 16 / 10;
      overflow: hidden;
      background: var(--mat-sys-surface-container-high);
    }

    img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    img.fallback {
      object-fit: cover;
      background: #e0f7fa;
    }

    mat-card-content {
      flex: 1;
    }

    .meta {
      display: flex;
      flex-wrap: wrap;
      gap: 0.75rem;
      margin: 0 0 0.75rem;
      color: var(--mat-sys-on-surface-variant);
      font-size: 0.875rem;
    }
  `,
})
export class DestinationCardComponent {
  readonly destination = input.required<DestinationListItem>();
  readonly imageFailed = signal(false);

  onImageError(): void {
    this.imageFailed.set(true);
  }
}
