import { BreakpointObserver } from '@angular/cdk/layout';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatStepperModule } from '@angular/material/stepper';
import { Router, RouterLink } from '@angular/router';
import { switchMap } from 'rxjs';
import { Interest } from '../../core/models/category.models';
import {
  DEMO_CLIMATES,
  SUPPORTED_TRIP_TYPES,
  TRAVEL_PERIODS,
} from '../../core/models/preference.models';
import { InterestService } from '../../core/services/interest.service';
import { PreferenceService } from '../../core/services/preference.service';
import { RecommendationService } from '../../core/services/recommendation.service';
import { TripPlanningStateService } from '../../core/services/trip-planning-state.service';
import { extractApiErrorMessage } from '../../core/utils/api-error.util';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-manual-questionnaire',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatStepperModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatChipsModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    LoadingSpinnerComponent,
  ],
  template: `
    <header class="page-header">
      <a mat-button routerLink="/plan-trip" class="back">
        <mat-icon>arrow_back</mat-icon>
        Back
      </a>
      <h1>Trip preferences</h1>
      <p>All fields are optional except where noted. Skip anything that does not matter to you.</p>
    </header>

    @if (interestsLoading()) {
      <app-loading-spinner message="Loading interests..." />
    } @else {
      <mat-stepper [orientation]="stepperOrientation()" linear #stepper>
        <mat-step [stepControl]="budgetGroup" label="Budget">
          <form [formGroup]="budgetGroup">
            <mat-form-field appearance="outline">
              <mat-label>Budget (EUR)</mat-label>
              <input matInput type="number" formControlName="budget" min="0" step="50" />
              <mat-hint>Total trip budget in euros, if you have one in mind.</mat-hint>
            </mat-form-field>
            <div class="step-actions">
              <button mat-flat-button color="primary" matStepperNext type="button">Next</button>
            </div>
          </form>
        </mat-step>

        <mat-step [stepControl]="durationGroup" label="Duration">
          <form [formGroup]="durationGroup">
            <mat-form-field appearance="outline">
              <mat-label>Duration (days)</mat-label>
              <input matInput type="number" formControlName="duration" min="1" max="60" />
              <mat-hint>Between 1 and 60 days.</mat-hint>
              @if (durationGroup.controls.duration.hasError('min') || durationGroup.controls.duration.hasError('max')) {
                <mat-error>Enter a value between 1 and 60.</mat-error>
              }
            </mat-form-field>
            <div class="step-actions">
              <button mat-button matStepperPrevious type="button">Back</button>
              <button mat-flat-button color="primary" matStepperNext type="button">Next</button>
            </div>
          </form>
        </mat-step>

        <mat-step [stepControl]="tripTypeGroup" label="Trip type">
          <form [formGroup]="tripTypeGroup">
            <p class="step-hint">Pick the style that best matches this trip.</p>
            <div class="trip-type-grid">
              @for (type of tripTypes; track type) {
                <button
                  type="button"
                  class="trip-type-card"
                  [class.selected]="tripTypeGroup.controls.tripType.value === type"
                  (click)="selectTripType(type)"
                >
                  {{ type }}
                </button>
              }
            </div>
            <div class="step-actions">
              <button mat-button matStepperPrevious type="button">Back</button>
              <button mat-flat-button color="primary" matStepperNext type="button">Next</button>
            </div>
          </form>
        </mat-step>

        <mat-step label="Interests">
          <p class="step-hint">Select all activities and themes you care about.</p>
          <mat-chip-set aria-label="Interests">
            @for (interest of interests(); track interest.id) {
              <mat-chip
                [highlighted]="isInterestSelected(interest.id)"
                (click)="toggleInterest(interest.id)"
              >
                {{ interest.name }}
              </mat-chip>
            }
          </mat-chip-set>
          <div class="step-actions">
            <button mat-button matStepperPrevious type="button">Back</button>
            <button mat-flat-button color="primary" matStepperNext type="button">Next</button>
          </div>
        </mat-step>

        <mat-step [stepControl]="periodGroup" label="When">
          <form [formGroup]="periodGroup">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Travel month</mat-label>
              <mat-select formControlName="travelPeriod">
                <mat-option [value]="null">No preference</mat-option>
                @for (month of travelPeriods; track month) {
                  <mat-option [value]="month">{{ month }}</mat-option>
                }
              </mat-select>
            </mat-form-field>
            <div class="step-actions">
              <button mat-button matStepperPrevious type="button">Back</button>
              <button mat-flat-button color="primary" matStepperNext type="button">Next</button>
            </div>
          </form>
        </mat-step>

        <mat-step [stepControl]="climateGroup" label="Climate">
          <form [formGroup]="climateGroup">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Preferred climate</mat-label>
              <mat-select formControlName="preferredClimate">
                <mat-option [value]="null">No preference</mat-option>
                @for (climate of climates; track climate) {
                  <mat-option [value]="climate">{{ climate }}</mat-option>
                }
              </mat-select>
            </mat-form-field>
            <div class="step-actions">
              <button mat-button matStepperPrevious type="button">Back</button>
              <button mat-flat-button color="primary" matStepperNext type="button">Next</button>
            </div>
          </form>
        </mat-step>

        <mat-step label="Review">
          <mat-card class="review-card">
            <mat-card-content>
              <dl class="review-list">
                <div>
                  <dt>Budget</dt>
                  <dd>{{ reviewBudget() }}</dd>
                </div>
                <div>
                  <dt>Duration</dt>
                  <dd>{{ reviewDuration() }}</dd>
                </div>
                <div>
                  <dt>Trip type</dt>
                  <dd>{{ tripTypeGroup.controls.tripType.value || 'Not specified' }}</dd>
                </div>
                <div>
                  <dt>Interests</dt>
                  <dd>{{ reviewInterests() }}</dd>
                </div>
                <div>
                  <dt>Travel month</dt>
                  <dd>{{ periodGroup.controls.travelPeriod.value || 'Not specified' }}</dd>
                </div>
                <div>
                  <dt>Climate</dt>
                  <dd>{{ climateGroup.controls.preferredClimate.value || 'Not specified' }}</dd>
                </div>
              </dl>
            </mat-card-content>
          </mat-card>

          @if (submitError()) {
            <p class="error">{{ submitError() }}</p>
          }

          <div class="step-actions">
            <button mat-button matStepperPrevious type="button" [disabled]="submitting()">Back</button>
            <button
              mat-flat-button
              color="primary"
              type="button"
              [disabled]="submitting() || durationGroup.invalid"
              (click)="generate()"
            >
              @if (submitting()) {
                <mat-spinner diameter="20" />
                Finding your best destinations...
              } @else {
                Generate recommendations
              }
            </button>
          </div>
        </mat-step>
      </mat-stepper>
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

    .back {
      margin: 0 0 0.5rem -0.5rem;
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

    .step-hint {
      margin: 0 0 1rem;
      color: var(--mat-sys-on-surface-variant);
    }

    .step-actions {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
      margin-top: 1.25rem;
    }

    .step-actions button mat-spinner {
      display: inline-block;
      margin-right: 0.35rem;
    }

    .trip-type-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
      gap: 0.75rem;
    }

    .trip-type-card {
      border: 2px solid rgba(13, 115, 119, 0.25);
      border-radius: 12px;
      padding: 1rem 0.75rem;
      background: var(--mat-sys-surface);
      cursor: pointer;
      font: inherit;
      transition: border-color 0.15s, background 0.15s;
    }

    .trip-type-card.selected {
      border-color: #0d7377;
      background: rgba(13, 115, 119, 0.08);
      font-weight: 600;
    }

    .full-width {
      width: 100%;
      max-width: 320px;
    }

    .review-card {
      margin-bottom: 1rem;
    }

    .review-list {
      margin: 0;
      display: grid;
      gap: 0.75rem;
    }

    .review-list dt {
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.04em;
      color: var(--mat-sys-on-surface-variant);
    }

    .review-list dd {
      margin: 0.15rem 0 0;
      font-weight: 500;
    }

    .error {
      color: var(--mat-sys-error);
    }

    mat-chip {
      cursor: pointer;
    }
  `,
})
export class ManualQuestionnaireComponent {
  private readonly fb = inject(FormBuilder);
  private readonly interestService = inject(InterestService);
  private readonly preferenceService = inject(PreferenceService);
  private readonly recommendationService = inject(RecommendationService);
  private readonly tripState = inject(TripPlanningStateService);
  private readonly router = inject(Router);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly destroyRef = inject(DestroyRef);

  readonly tripTypes = SUPPORTED_TRIP_TYPES;
  readonly travelPeriods = TRAVEL_PERIODS;
  readonly climates = DEMO_CLIMATES;

  readonly interests = signal<Interest[]>([]);
  readonly interestsLoading = signal(true);
  readonly selectedInterestIds = signal<number[]>([]);
  readonly stepperOrientation = signal<'horizontal' | 'vertical'>('horizontal');
  readonly submitting = signal(false);
  readonly submitError = signal<string | null>(null);

  readonly budgetGroup = this.fb.group({
    budget: this.fb.control<number | null>(null, { validators: [Validators.min(0)] }),
  });

  readonly durationGroup = this.fb.group({
    duration: this.fb.control<number | null>(null, {
      validators: [Validators.min(1), Validators.max(60)],
    }),
  });

  readonly tripTypeGroup = this.fb.group({
    tripType: this.fb.control<string | null>(null),
  });

  readonly periodGroup = this.fb.group({
    travelPeriod: this.fb.control<string | null>(null),
  });

  readonly climateGroup = this.fb.group({
    preferredClimate: this.fb.control<string | null>(null),
  });

  constructor() {
    this.interestService.getAll().subscribe({
      next: (list) => {
        this.interests.set(list);
        this.interestsLoading.set(false);
      },
      error: () => {
        this.interests.set([]);
        this.interestsLoading.set(false);
      },
    });

    this.breakpointObserver
      .observe('(max-width: 599px)')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((state) => {
        this.stepperOrientation.set(state.matches ? 'vertical' : 'horizontal');
      });
  }

  selectTripType(type: string): void {
    const current = this.tripTypeGroup.controls.tripType.value;
    this.tripTypeGroup.controls.tripType.setValue(current === type ? null : type);
  }

  isInterestSelected(id: number): boolean {
    return this.selectedInterestIds().includes(id);
  }

  toggleInterest(id: number): void {
    const current = this.selectedInterestIds();
    if (current.includes(id)) {
      this.selectedInterestIds.set(current.filter((x) => x !== id));
    } else {
      this.selectedInterestIds.set([...current, id]);
    }
  }

  reviewBudget(): string {
    const value = this.budgetGroup.controls.budget.value;
    return value != null && value > 0 ? `${value} EUR` : 'Not specified';
  }

  reviewDuration(): string {
    const value = this.durationGroup.controls.duration.value;
    return value != null ? `${value} days` : 'Not specified';
  }

  reviewInterests(): string {
    const ids = this.selectedInterestIds();
    if (!ids.length) {
      return 'None selected';
    }
    const names = this.interests()
      .filter((i) => ids.includes(i.id))
      .map((i) => i.name);
    return names.length ? names.join(', ') : `${ids.length} selected`;
  }

  generate(): void {
    if (this.submitting() || this.durationGroup.invalid) {
      return;
    }

    this.submitting.set(true);
    this.submitError.set(null);

    const budget = this.budgetGroup.controls.budget.value;
    const duration = this.durationGroup.controls.duration.value;

    this.preferenceService
      .createPreference({
        budget: budget != null && budget > 0 ? budget : null,
        duration: duration != null && duration > 0 ? duration : null,
        tripType: this.tripTypeGroup.controls.tripType.value,
        travelPeriod: this.periodGroup.controls.travelPeriod.value,
        preferredClimate: this.climateGroup.controls.preferredClimate.value,
        interestIds: this.selectedInterestIds(),
      })
      .pipe(switchMap((pref) => this.recommendationService.generateRecommendation(pref.id, 5)))
      .subscribe({
        next: (recommendation) => {
          this.tripState.setLastGenerated(recommendation);
          this.submitting.set(false);
          void this.router.navigate(['/recommendations', recommendation.id]);
        },
        error: (err) => {
          this.submitting.set(false);
          this.submitError.set(extractApiErrorMessage(err, 'Could not generate recommendations.'));
        },
      });
  }
}
