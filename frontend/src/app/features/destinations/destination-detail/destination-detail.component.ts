import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';

import { Component, inject, signal } from '@angular/core';

import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { MatButtonModule } from '@angular/material/button';

import { MatCardModule } from '@angular/material/card';

import { MatChipsModule } from '@angular/material/chips';

import { MatIconModule } from '@angular/material/icon';

import { MatDialog } from '@angular/material/dialog';

import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { EMPTY, switchMap } from 'rxjs';

import { AuthService } from '../../../core/services/auth.service';

import { DestinationService } from '../../../core/services/destination.service';

import { FavoriteService } from '../../../core/services/favorite.service';

import { WeatherService } from '../../../core/services/weather.service';

import { DestinationDetails } from '../../../core/models/destination.models';

import { WeatherResponse } from '../../../core/models/weather.models';

import { FavoriteToggleComponent } from '../../../shared/components/favorite-toggle/favorite-toggle.component';

import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';

import { DestinationMapComponent } from '../../../shared/components/destination-map/destination-map.component';

import { extractApiErrorMessage } from '../../../core/utils/api-error.util';

import {
  GenerateItineraryDialogComponent,
  GenerateItineraryDialogData,
} from '../../itineraries/generate-itinerary-dialog.component';



@Component({

  selector: 'app-destination-detail',

  imports: [

    CurrencyPipe,

    DatePipe,

    DecimalPipe,

    RouterLink,

    MatCardModule,

    MatButtonModule,

    MatChipsModule,

    MatIconModule,

    FavoriteToggleComponent,

    LoadingSpinnerComponent,

    DestinationMapComponent,

  ],

  template: `

    @if (loading()) {

      <app-loading-spinner message="Loading destination..." />

    } @else if (error()) {

      <p class="error">{{ error() }}</p>

      <a mat-button routerLink="/destinations">Back to list</a>

    } @else if (destination(); as item) {

      <div class="header-row">

        <a mat-button routerLink="/destinations">

          <mat-icon>arrow_back</mat-icon>

          Back

        </a>

        <div class="header-actions">
          @if (auth.isAuthenticated()) {
            <button mat-flat-button color="primary" type="button" (click)="planTrip(item)">
              <mat-icon>event_note</mat-icon>
              Plan this trip
            </button>
          } @else {
            <a
              mat-stroked-button
              routerLink="/login"
              [queryParams]="{ returnUrl: router.url }"
            >
              Sign in to plan this trip
            </a>
          }

          <app-favorite-toggle

            [destinationId]="item.id"

            [initialFavorite]="favorite()"

          />
        </div>

      </div>



      <mat-card class="hero-card">

        <img

          [src]="item.imageUrl || placeholder"

          [alt]="item.name"

          class="hero-image"

          (error)="onImageError($event)"

        />

        <mat-card-header>

          <mat-card-title>{{ item.name }}</mat-card-title>

          <mat-card-subtitle>{{ item.city }}, {{ item.country }}</mat-card-subtitle>

        </mat-card-header>

      </mat-card>



      <mat-card class="section-card">

        <mat-card-content>

          <h3 class="section-title">About</h3>

          @if (item.description) {

            <p>{{ item.description }}</p>

          } @else {

            <p class="muted">No description available.</p>

          }

          <div class="facts">

            <span>{{ item.averageDailyCost | currency: 'EUR' : 'symbol' : '1.0-0' }}/day</span>

            <span>Popularity {{ item.popularity }}</span>

            @if (item.climate) {

              <span>{{ item.climate }}</span>

            }

            <span>Added {{ item.createdAt | date: 'mediumDate' }}</span>

          </div>

          @if (item.categories.length) {

            <mat-chip-set aria-label="Categories">

              @for (category of item.categories; track category.id) {

                <mat-chip>{{ category.name }}</mat-chip>

              }

            </mat-chip-set>

          }

          @if (item.interests.length) {

            <mat-chip-set aria-label="Interests">

              @for (interest of item.interests; track interest.id) {

                <mat-chip>{{ interest.name }}</mat-chip>

              }

            </mat-chip-set>

          }

        </mat-card-content>

      </mat-card>



      <mat-card class="section-card">

        <mat-card-header>

          <mat-card-title>Weather</mat-card-title>

        </mat-card-header>

        <mat-card-content>

          <p class="weather-disclaimer muted">

            Weather data is provided for current travel planning and does

            not affect the recommendation score.

          </p>

          @if (weatherLoading()) {

            <app-loading-spinner message="Loading weather..." />

          } @else if (weatherError()) {

            <p class="error">{{ weatherError() }}</p>

          } @else if (weather(); as wx) {

            @if (wx.isStale) {

              <p class="muted stale-note">Data may be outdated.</p>

            }

            <div class="weather-current">

              @if (weatherIconUrl(wx.current.icon); as iconUrl) {

                <img [src]="iconUrl" [alt]="wx.current.description" class="weather-icon" />

              }

              <div>

                @if (displayTemperature(wx.current.temperature); as temp) {

                  <p class="weather-temp">{{ temp | number: '1.0-0' }}°C</p>

                }

                <p class="weather-desc">{{ wx.current.description }}</p>

                <div class="weather-meta">

                  @if (displayTemperature(wx.current.feelsLike); as feels) {

                    <span>Feels like {{ feels | number: '1.0-0' }}°C</span>

                  }

                  @if (

                    displayTemperature(wx.current.minTemperature) != null &&

                    displayTemperature(wx.current.maxTemperature) != null

                  ) {

                    <span>

                      {{ displayTemperature(wx.current.minTemperature)! | number: '1.0-0' }}–{{

                        displayTemperature(wx.current.maxTemperature)! | number: '1.0-0'

                      }}°C

                    </span>

                  }

                  <span>Humidity {{ wx.current.humidity }}%</span>

                  <span>Wind {{ wx.current.windSpeed | number: '1.0-0' }} km/h</span>

                  <span>Observed {{ wx.current.observedAt | date: 'short' }}</span>

                </div>

              </div>

            </div>

            <p class="weather-updated muted">

              Weather updated: {{ wx.fetchedAt | date: 'short' }}

              @if (wx.isStale) {

                <span> (stale cache)</span>

              } @else if (wx.isCached) {

                <span> (cached)</span>

              }

            </p>

            @if (wx.forecast.length) {

              <h4 class="forecast-heading">Forecast</h4>

              <div class="forecast-grid">

                @for (day of wx.forecast; track day.date) {

                  <article class="forecast-card">

                    <p class="forecast-date">{{ day.date | date: 'EEE, MMM d' }}</p>

                    @if (weatherIconUrl(day.icon); as dayIcon) {

                      <img [src]="dayIcon" [alt]="day.description" class="forecast-icon" />

                    }

                    @if (

                      displayTemperature(day.minTemperature) != null &&

                      displayTemperature(day.maxTemperature) != null

                    ) {

                      <p class="forecast-temps">

                        {{ displayTemperature(day.minTemperature)! | number: '1.0-0' }}–{{

                          displayTemperature(day.maxTemperature)! | number: '1.0-0'

                        }}°C

                      </p>

                    }

                    <p class="muted forecast-desc">{{ day.description }}</p>

                  </article>

                }

              </div>

            }

          }

        </mat-card-content>

      </mat-card>



      <mat-card class="section-card">

        <mat-card-header>

          <mat-card-title>Location</mat-card-title>

          <mat-card-subtitle>{{ item.city }}, {{ item.country }}</mat-card-subtitle>

        </mat-card-header>

        <mat-card-content>

          <app-destination-map

            [destinationName]="item.name"

            [city]="item.city"

            [country]="item.country"

            [latitude]="item.latitude"

            [longitude]="item.longitude"

            [attractions]="item.attractions"

          />

        </mat-card-content>

      </mat-card>



      <div class="columns">

        <mat-card>

          <mat-card-header>

            <mat-card-title>Attractions</mat-card-title>

          </mat-card-header>

          <mat-card-content>

            @if (!item.attractions.length) {

              <p class="muted">No attractions listed.</p>

            } @else {

              <div class="attraction-grid">

                @for (attraction of item.attractions; track attraction.id) {

                  <article class="attraction-card">

                    <img

                      [src]="attraction.imageUrl || placeholder"

                      [alt]="attraction.name"

                      (error)="onImageError($event)"

                    />

                    <h4>{{ attraction.name }}</h4>

                    @if (attraction.location) {

                      <p class="muted">{{ attraction.location }}</p>

                    }

                    @if (attraction.description) {

                      <p>{{ attraction.description }}</p>

                    }

                  </article>

                }

              </div>

            }

          </mat-card-content>

        </mat-card>



        <mat-card>

          <mat-card-header>

            <mat-card-title>Activities</mat-card-title>

          </mat-card-header>

          <mat-card-content>

            @if (!item.activities.length) {

              <p class="muted">No activities listed.</p>

            } @else {

              <div class="activity-list">

                @for (activity of item.activities; track activity.id) {

                  <article class="activity-card">

                    <h4>{{ activity.name }}</h4>

                    @if (activity.provider) {

                      <p class="muted">Provider: {{ activity.provider }}</p>

                    }

                    @if (activity.price != null) {

                      <p>

                        {{ activity.price | currency: activity.currency || 'EUR' : 'symbol' : '1.0-2' }}

                      </p>

                    }

                    @if (activity.description) {

                      <p>{{ activity.description }}</p>

                    }

                    @if (isValidUrl(activity.externalUrl)) {

                      <a [href]="activity.externalUrl!" target="_blank" rel="noopener noreferrer">

                        Open link

                      </a>

                    }

                  </article>

                }

              </div>

            }

          </mat-card-content>

        </mat-card>

      </div>

    }

  `,

  styles: `

    .header-row {

      display: flex;

      justify-content: space-between;

      align-items: center;

      margin-bottom: 0.5rem;

      gap: 0.5rem;

      flex-wrap: wrap;

    }

    .header-actions {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      flex-wrap: wrap;
    }



    .hero-card,

    .section-card {

      margin-bottom: 1rem;

      overflow: hidden;

    }



    .hero-image {

      width: 100%;

      max-height: 320px;

      object-fit: cover;

    }



    .facts {

      display: flex;

      flex-wrap: wrap;

      gap: 0.75rem 1rem;

      margin: 1rem 0;

      color: var(--mat-sys-on-surface-variant);

      font-size: 0.9rem;

    }



    .columns {

      display: grid;

      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));

      gap: 1rem;

    }



    .section-title {

      margin: 0.5rem 0;

      font-family: Fraunces, Georgia, serif;

    }



    .weather-disclaimer {

      margin: 0 0 1rem;

      font-size: 0.875rem;

    }

    .weather-updated {

      margin: 0.75rem 0 0;

      font-size: 0.8rem;

    }



    .weather-current {

      display: flex;

      gap: 1rem;

      align-items: flex-start;

    }



    .weather-icon {

      width: 72px;

      height: 72px;

    }



    .weather-temp {

      margin: 0;

      font-size: 2rem;

      font-weight: 600;

      line-height: 1.1;

    }



    .weather-desc {

      margin: 0.25rem 0 0.5rem;

      text-transform: capitalize;

    }



    .weather-meta {

      display: flex;

      flex-wrap: wrap;

      gap: 0.5rem 1rem;

      font-size: 0.875rem;

      color: var(--mat-sys-on-surface-variant);

    }



    .forecast-heading {

      margin: 1.25rem 0 0.75rem;

      font-size: 1rem;

    }



    .forecast-grid {

      display: grid;

      grid-template-columns: repeat(auto-fill, minmax(120px, 1fr));

      gap: 0.75rem;

    }



    .forecast-card {

      padding: 0.75rem;

      border-radius: 8px;

      background: var(--mat-sys-surface-container-low);

      text-align: center;

    }



    .forecast-date {

      margin: 0 0 0.25rem;

      font-size: 0.8rem;

      font-weight: 600;

    }



    .forecast-icon {

      width: 48px;

      height: 48px;

    }



    .forecast-temps {

      margin: 0.25rem 0;

      font-weight: 600;

    }



    .forecast-desc {

      margin: 0;

      font-size: 0.75rem;

      text-transform: capitalize;

    }



    .stale-note {

      margin: 0 0 0.75rem;

      font-size: 0.875rem;

    }



    .attraction-grid,

    .activity-list {

      display: grid;

      gap: 1rem;

    }



    .attraction-card img {

      width: 100%;

      height: 140px;

      object-fit: cover;

      border-radius: 8px;

    }



    .attraction-card h4,

    .activity-card h4 {

      margin: 0.5rem 0 0.25rem;

    }



    .muted {

      color: var(--mat-sys-on-surface-variant);

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

export class DestinationDetailComponent {

  private readonly route = inject(ActivatedRoute);

  private readonly destinationService = inject(DestinationService);

  private readonly weatherService = inject(WeatherService);

  private readonly favoriteService = inject(FavoriteService);

  private readonly dialog = inject(MatDialog);

  protected readonly auth = inject(AuthService);

  protected readonly router = inject(Router);

  readonly placeholder = 'assets/images/destination-placeholder.svg';

  readonly destination = signal<DestinationDetails | null>(null);

  readonly favorite = signal<boolean | null>(null);

  readonly loading = signal(true);

  readonly error = signal<string | null>(null);

  readonly weather = signal<WeatherResponse | null>(null);

  readonly weatherLoading = signal(false);

  readonly weatherError = signal<string | null>(null);



  onImageError(event: Event): void {

    const img = event.target as HTMLImageElement;

    img.src = this.placeholder;

  }

  planTrip(item: DestinationDetails): void {
    const data: GenerateItineraryDialogData = {
      destinationId: item.id,
      destinationName: item.name,
    };

    this.dialog.open<GenerateItineraryDialogComponent, GenerateItineraryDialogData, number>(
      GenerateItineraryDialogComponent,
      { data, width: '520px' }
    );
  }



  isValidUrl(url: string | null | undefined): boolean {

    if (!url?.trim()) {

      return false;

    }

    try {

      const parsed = new URL(url);

      return parsed.protocol === 'http:' || parsed.protocol === 'https:';

    } catch {

      return false;

    }

  }



  displayTemperature(value: number | null | undefined): number | null {

    if (value == null || !Number.isFinite(value)) {

      return null;

    }

    return value;

  }



  weatherIconUrl(icon: string | null | undefined): string | null {

    if (!icon?.trim()) {

      return null;

    }

    const trimmed = icon.trim();

    if (trimmed.startsWith('http://') || trimmed.startsWith('https://')) {

      return trimmed;

    }

    return `https://openweathermap.org/img/wn/${trimmed}@2x.png`;

  }



  private loadWeather(destinationId: number): void {

    this.weatherLoading.set(true);

    this.weatherError.set(null);

    this.weather.set(null);



    this.weatherService.getByDestinationId(destinationId).subscribe({

      next: (response) => {

        this.weather.set(response);

        this.weatherLoading.set(false);

      },

      error: (err) => {

        this.weatherError.set(

          extractApiErrorMessage(err, 'Weather information is currently unavailable.')

        );

        this.weatherLoading.set(false);

      },

    });

  }



  constructor() {

    this.route.paramMap

      .pipe(

        switchMap((params) => {

          this.loading.set(true);

          this.error.set(null);

          this.favorite.set(null);

          this.weather.set(null);

          this.weatherError.set(null);

          this.weatherLoading.set(false);



          const id = Number(params.get('id'));

          if (!Number.isFinite(id)) {

            this.error.set('Invalid destination id.');

            this.loading.set(false);

            return EMPTY;

          }



          this.loadWeather(id);

          return this.destinationService.getById(id);

        }),

        takeUntilDestroyed()

      )

      .subscribe({

        next: (details) => {

          this.destination.set(details);

          this.loading.set(false);

          if (this.auth.isAuthenticated()) {

            this.favoriteService.getStatus(details.id).subscribe({

              next: (status) => this.favorite.set(status.isFavorite),

            });

          }

        },

        error: (err) => {

          if (this.error()) {

            return;

          }

          this.error.set(extractApiErrorMessage(err, 'Destination not found.'));

          this.loading.set(false);

        },

      });

  }

}

