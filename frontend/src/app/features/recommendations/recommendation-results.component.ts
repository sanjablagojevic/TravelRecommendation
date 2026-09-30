import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog } from '@angular/material/dialog';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { switchMap } from 'rxjs';
import {
  Recommendation,
  RecommendationItem,
  ScoreComponent,
} from '../../core/models/recommendation.models';
import { PreferenceService } from '../../core/services/preference.service';
import { RecommendationService } from '../../core/services/recommendation.service';
import { TripPlanningStateService } from '../../core/services/trip-planning-state.service';
import { extractApiErrorMessage } from '../../core/utils/api-error.util';
import { FavoriteToggleComponent } from '../../shared/components/favorite-toggle/favorite-toggle.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import {
  GenerateItineraryDialogComponent,
  GenerateItineraryDialogData,
} from '../itineraries/generate-itinerary-dialog.component';

@Component({
  selector: 'app-recommendation-results',
  imports: [
    CurrencyPipe,
    DecimalPipe,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatChipsModule,
    MatExpansionModule,
    MatIconModule,
    MatProgressSpinnerModule,
    FavoriteToggleComponent,
    LoadingSpinnerComponent,
  ],
  template: `
    <header class="page-header">
      <h1>Your recommended destinations</h1>
      <p class="score-note">
        Match scores reflect only the preferences you provided. Criteria you did not specify are
        excluded from the percentage, not counted as zero.
      </p>
    </header>

    @if (loading()) {
      <app-loading-spinner message="Loading recommendations..." />
    } @else if (error()) {
      <p class="error">{{ error() }}</p>
      <a mat-button routerLink="/plan-trip">Plan another trip</a>
    } @else if (items().length) {
      <div class="toolbar">
        <button
          mat-stroked-button
          color="primary"
          type="button"
          [disabled]="aiLoading()"
          (click)="generateAiExplanations()"
        >
          @if (aiLoading()) {
            <mat-spinner diameter="18" />
            Generating AI explanations...
          } @else {
            Generate AI explanations
          }
        </button>
        <a mat-button routerLink="/recommendation-history">View history</a>
      </div>

      <div class="results">
        @for (item of items(); track item.rank) {
          <mat-card class="result-card">
            @if (item.rank === 1) {
              <span class="best-badge">#1 Best match</span>
            } @else {
              <span class="rank-badge">#{{ item.rank }}</span>
            }

            <div class="card-layout">
              <div class="image-wrap">
                @if (item.destination.imageUrl) {
                  <img [src]="item.destination.imageUrl" [alt]="item.destination.name" loading="lazy" />
                } @else {
                  <div class="image-fallback">
                    <mat-icon>landscape</mat-icon>
                  </div>
                }
              </div>

              <div class="card-body">
                <div class="title-row">
                  <div>
                    <h2>{{ item.destination.name }}</h2>
                    <p class="location">
                      {{ item.destination.city }}, {{ item.destination.country }}
                    </p>
                  </div>
                  <app-favorite-toggle [destinationId]="item.destination.id" />
                </div>

                <p class="match">
                  <strong>{{ item.matchPercentage | number: '1.2-2' }}%</strong> match
                </p>

                @if (item.estimatedCost != null) {
                  <p class="cost">
                    Estimated trip cost:
                    <strong>{{ item.estimatedCost | currency: 'EUR' : 'symbol' : '1.0-0' }}</strong>
                    <span class="disclaimer"
                      >Based on estimated average daily cost stored in the application; not a
                      real-time travel price.</span
                    >
                  </p>
                }

                @if (item.matchedInterests.length) {
                  <div class="chip-row">
                    <span class="chip-label">Matched interests</span>
                    <mat-chip-set>
                      @for (name of item.matchedInterests; track name) {
                        <mat-chip class="matched">{{ name }}</mat-chip>
                      }
                    </mat-chip-set>
                  </div>
                }

                @if (item.missingInterests.length) {
                  <div class="chip-row">
                    <span class="chip-label">Not matched</span>
                    <mat-chip-set>
                      @for (name of item.missingInterests; track name) {
                        <mat-chip class="missing">{{ name }}</mat-chip>
                      }
                    </mat-chip-set>
                  </div>
                }

                <section class="explanation-block">
                  <h3>Score explanation</h3>
                  <p class="explanation">{{ item.explanation }}</p>
                </section>

                @if (item.aiExplanation) {
                  <section class="explanation-block ai">
                    <h3>AI explanation</h3>
                    <p class="explanation">{{ item.aiExplanation }}</p>
                  </section>
                }

                <mat-expansion-panel class="breakdown-panel">
                  <mat-expansion-panel-header>
                    <mat-panel-title>Why this match?</mat-panel-title>
                  </mat-expansion-panel-header>

                  @if (item.scoreBreakdown?.components?.length) {
                    <ul class="breakdown-list">
                      @for (component of item.scoreBreakdown!.components; track component.name) {
                        <li>
                          <span class="component-name">{{ component.name }}</span>
                          @if (component.isActive) {
                            <span
                              >{{ formatComponentScore(component) }}
                              <span class="weight"
                                >(weight {{ component.weight | number: '1.0-2' }})</span
                              ></span
                            >
                          } @else {
                            <span class="inactive">Not specified</span>
                          }
                        </li>
                      }
                    </ul>
                  } @else {
                    <p class="muted">
                      Detailed score breakdown is available right after generation. Historical
                      opens keep the saved score without recomputing.
                    </p>
                  }
                </mat-expansion-panel>

                <div class="card-actions">
                  <a mat-button color="primary" [routerLink]="['/destinations', item.destination.id]">
                    View destination
                  </a>
                  <button
                    mat-stroked-button
                    color="primary"
                    type="button"
                    (click)="createItinerary(item)"
                  >
                    Create itinerary
                  </button>
                </div>
              </div>
            </div>
          </mat-card>
        }
      </div>
    } @else {
      <p class="muted">No recommendations in this result.</p>
    }
  `,
  styles: `
    :host {
      display: block;
      width: min(960px, 100%);
      margin: 0 auto;
      padding: 1.5rem 1rem 2.5rem;
      box-sizing: border-box;
    }

    h1 {
      font-family: 'Fraunces', Georgia, serif;
      margin: 0 0 0.5rem;
      color: #0d4f52;
    }

    .score-note {
      margin: 0 0 1.25rem;
      color: var(--mat-sys-on-surface-variant);
      line-height: 1.5;
      max-width: 40rem;
    }

    .toolbar {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
      align-items: center;
      margin-bottom: 1.25rem;
    }

    .toolbar button {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
    }

    .results {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    .result-card {
      position: relative;
      overflow: hidden;
    }

    .best-badge {
      position: absolute;
      top: 12px;
      left: 12px;
      z-index: 1;
      background: #0d7377;
      color: #fff;
      font-size: 0.75rem;
      font-weight: 600;
      padding: 0.35rem 0.65rem;
      border-radius: 999px;
    }

    .rank-badge {
      position: absolute;
      top: 12px;
      left: 12px;
      z-index: 1;
      background: rgba(0, 0, 0, 0.55);
      color: #fff;
      font-size: 0.75rem;
      font-weight: 600;
      padding: 0.35rem 0.65rem;
      border-radius: 999px;
    }

    .card-layout {
      display: grid;
      grid-template-columns: minmax(160px, 240px) 1fr;
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
      min-height: 140px;
      object-fit: cover;
      border-radius: 8px;
    }

    .image-fallback {
      min-height: 140px;
      background: linear-gradient(145deg, #0d7377 0%, #2a9d8f 100%);
      border-radius: 8px;
      display: flex;
      align-items: center;
      justify-content: center;
      color: rgba(255, 255, 255, 0.85);
    }

    .image-fallback mat-icon {
      font-size: 3rem;
      width: 3rem;
      height: 3rem;
    }

    h2 {
      margin: 0;
      font-family: 'Fraunces', Georgia, serif;
      font-size: 1.35rem;
    }

    .title-row {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 0.5rem;
    }

    .location {
      margin: 0.15rem 0 0;
      color: var(--mat-sys-on-surface-variant);
    }

    .match {
      margin: 0.75rem 0 0.35rem;
      font-size: 1.05rem;
    }

    .cost {
      margin: 0 0 0.75rem;
      font-size: 0.9375rem;
    }

    .disclaimer {
      display: block;
      font-size: 0.75rem;
      color: var(--mat-sys-on-surface-variant);
    }

    .chip-row {
      margin-bottom: 0.5rem;
    }

    .chip-label {
      display: block;
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.04em;
      color: var(--mat-sys-on-surface-variant);
      margin-bottom: 0.25rem;
    }

    mat-chip.matched {
      --mdc-chip-elevated-container-color: rgba(42, 157, 143, 0.15);
    }

    mat-chip.missing {
      --mdc-chip-elevated-container-color: rgba(0, 0, 0, 0.06);
    }

    .explanation {
      margin: 0.25rem 0 0.75rem;
      line-height: 1.55;
    }

    .explanation-block h3 {
      margin: 0.75rem 0 0;
      font-size: 0.8rem;
      text-transform: uppercase;
      letter-spacing: 0.04em;
      color: var(--mat-sys-on-surface-variant);
      font-family: 'Source Sans 3', sans-serif;
      font-weight: 600;
    }

    .explanation-block.ai {
      padding: 0.75rem;
      border-radius: 8px;
      background: rgba(13, 115, 119, 0.06);
    }

    .weight {
      color: var(--mat-sys-on-surface-variant);
      font-size: 0.8rem;
    }

    .breakdown-panel {
      margin: 0.5rem 0 0.75rem;
      box-shadow: none;
      border: 1px solid rgba(0, 0, 0, 0.08);
    }

    .breakdown-list {
      list-style: none;
      margin: 0;
      padding: 0;
      display: grid;
      gap: 0.35rem;
    }

    .breakdown-list li {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      font-size: 0.875rem;
    }

    .component-name {
      font-weight: 500;
    }

    .inactive {
      color: var(--mat-sys-on-surface-variant);
      font-style: italic;
    }

    .card-actions {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
      align-items: center;
    }

    .error {
      color: var(--mat-sys-error);
    }

    .muted {
      color: var(--mat-sys-on-surface-variant);
    }
  `,
})
export class RecommendationResultsComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly recommendationService = inject(RecommendationService);
  private readonly tripState = inject(TripPlanningStateService);
  private readonly preferenceService = inject(PreferenceService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly items = signal<RecommendationItem[]>([]);
  readonly recommendationId = signal<number | null>(null);
  readonly userPreferenceId = signal<number | null>(null);
  readonly preferredDuration = signal<number | null>(null);
  readonly aiLoading = signal(false);

  constructor() {
    this.route.paramMap
      .pipe(
        switchMap((params) => {
          this.loading.set(true);
          this.error.set(null);
          const id = Number(params.get('id'));
          if (!Number.isFinite(id)) {
            throw new Error('Invalid recommendation id');
          }
          this.recommendationId.set(id);
          return this.recommendationService.getRecommendation(id);
        }),
        takeUntilDestroyed()
      )
      .subscribe({
        next: (recommendation) => {
          this.applyRecommendation(recommendation);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(extractApiErrorMessage(err, 'Could not load recommendations.'));
          this.loading.set(false);
        },
      });
  }

  displayExplanation(item: RecommendationItem): string {
    return item.explanation;
  }

  formatComponentScore(component: ScoreComponent): string {
    if (component.score == null) {
      return '—';
    }
    return `${(component.score * 100).toFixed(1)}%`;
  }

  generateAiExplanations(): void {
    const id = this.recommendationId();
    if (id == null || this.aiLoading()) {
      return;
    }

    this.aiLoading.set(true);
    this.recommendationService.generateAiExplanations(id).subscribe({
      next: (response) => {
        this.items.update((current) =>
          current.map((item) => {
            const aiItem = response.items.find((x) => x.rank === item.rank);
            if (!aiItem?.aiExplanation) {
              return item;
            }
            return { ...item, aiExplanation: aiItem.aiExplanation };
          })
        );
        this.aiLoading.set(false);
        if (response.warning) {
          this.snackBar.open(response.warning, 'OK', { duration: 5000 });
        }
      },
      error: (err) => {
        this.aiLoading.set(false);
        this.snackBar.open(
          extractApiErrorMessage(err, 'AI explanations unavailable.'),
          'Dismiss',
          { duration: 5000 }
        );
      },
    });
  }

  createItinerary(item: RecommendationItem): void {
    const data: GenerateItineraryDialogData = {
      destinationId: item.destination.id,
      destinationName: item.destination.name,
      recommendationId: this.recommendationId(),
      userPreferenceId: this.userPreferenceId(),
      defaultDurationDays: this.preferredDuration(),
    };

    this.dialog.open<GenerateItineraryDialogComponent, GenerateItineraryDialogData, number>(
      GenerateItineraryDialogComponent,
      { data, width: '520px' }
    );
  }

  private applyRecommendation(recommendation: Recommendation): void {
    this.userPreferenceId.set(recommendation.userPreferenceId);
    this.loadPreferredDuration(recommendation.userPreferenceId);

    const cached = this.tripState.getLastGenerated(recommendation.id);
    const mergedItems = recommendation.items.map((item) => {
      const cachedItem = cached?.items.find((x) => x.rank === item.rank);
      const breakdown = item.scoreBreakdown ?? cachedItem?.scoreBreakdown ?? null;
      return breakdown ? { ...item, scoreBreakdown: breakdown } : item;
    });
    this.items.set([...mergedItems].sort((a, b) => a.rank - b.rank));
  }

  /** Trip length is stored on the preference, not on the recommendation, so it is fetched separately. */
  private loadPreferredDuration(userPreferenceId: number): void {
    this.preferredDuration.set(null);
    this.preferenceService.getPreference(userPreferenceId).subscribe({
      next: (preference) => this.preferredDuration.set(preference.duration ?? null),
      error: () => this.preferredDuration.set(null),
    });
  }
}
