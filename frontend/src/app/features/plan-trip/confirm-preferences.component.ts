import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router, RouterLink } from '@angular/router';
import { switchMap } from 'rxjs';
import { Interest } from '../../core/models/category.models';
import {
  DEMO_CLIMATES,
  SUPPORTED_TRIP_TYPES,
  TRAVEL_PERIODS,
} from '../../core/models/preference.models';
import { AiService } from '../../core/services/ai.service';
import { InterestService } from '../../core/services/interest.service';
import { RecommendationService } from '../../core/services/recommendation.service';
import { TripPlanningStateService } from '../../core/services/trip-planning-state.service';
import { extractApiErrorMessage } from '../../core/utils/api-error.util';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-confirm-preferences',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatChipsModule,
    MatIconModule,
    MatProgressSpinnerModule,
    LoadingSpinnerComponent,
  ],
  template: `
    <header class="page-header">
      <a mat-button routerLink="/plan-trip/ai" class="back">
        <mat-icon>arrow_back</mat-icon>
        Back
      </a>
      <h1>Review what we understood</h1>
      <p>Review and edit these preferences before continuing. AI extraction can be wrong.</p>
    </header>

    @if (interestsLoading()) {
      <app-loading-spinner message="Loading interests..." />
    } @else {
      @if (extractedCurrencyNote()) {
        <p class="currency-note">{{ extractedCurrencyNote() }}</p>
      }

      @if (requiresEurBudget()) {
        <p class="eur-notice">
          Our current recommendation engine compares budgets in <strong>EUR</strong>. Enter the
          amount in euros yourself — we do not invent exchange rates.
          @if (originalBudgetLabel()) {
            Detected: {{ originalBudgetLabel() }}.
          }
        </p>
      }

      <form [formGroup]="form" (ngSubmit)="confirm()" class="form">
        <div class="field-row">
          <mat-form-field appearance="outline">
            <mat-label>Budget (EUR)</mat-label>
            <input matInput type="number" formControlName="budget" min="0" step="50" />
            @if (requiresEurBudget() && form.controls.budget.hasError('required')) {
              <mat-error>Budget in EUR is required.</mat-error>
            }
          </mat-form-field>
          @if (displayCurrency()) {
            <span class="currency-badge">Original: {{ displayCurrency() }}</span>
          }
        </div>

        <mat-form-field appearance="outline">
          <mat-label>Duration (days)</mat-label>
          <input matInput type="number" formControlName="duration" min="1" max="60" />
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Trip type</mat-label>
          <mat-select formControlName="tripType">
            <mat-option [value]="null">No preference</mat-option>
            @for (type of tripTypes; track type) {
              <mat-option [value]="type">{{ type }}</mat-option>
            }
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Travel month</mat-label>
          <mat-select formControlName="travelPeriod">
            <mat-option [value]="null">No preference</mat-option>
            @for (month of travelPeriods; track month) {
              <mat-option [value]="month">{{ month }}</mat-option>
            }
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Preferred climate</mat-label>
          <mat-select formControlName="preferredClimate">
            <mat-option [value]="null">No preference</mat-option>
            @for (climate of climates; track climate) {
              <mat-option [value]="climate">{{ climate }}</mat-option>
            }
          </mat-select>
        </mat-form-field>

        <section class="interests-section">
          <h2>Interests</h2>
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
        </section>

        @if (unmappedPreferences().length) {
          <section class="unmapped">
            <h2>Could not map automatically</h2>
            <p>These details were noted but are not used in scoring yet:</p>
            <ul>
              @for (item of unmappedPreferences(); track item) {
                <li>{{ item }}</li>
              }
            </ul>
          </section>
        }

        @if (submitError()) {
          <p class="error">{{ submitError() }}</p>
        }

        <div class="actions">
          <button mat-flat-button color="primary" type="submit" [disabled]="submitting() || form.invalid">
            @if (submitting()) {
              <mat-spinner diameter="20" />
              Finding your best destinations...
            } @else {
              Confirm and find destinations
            }
          </button>
        </div>
      </form>
    }
  `,
  styles: `
    :host {
      display: block;
      width: min(720px, 100%);
      margin: 0 auto;
      padding: 1.5rem 1rem 2.5rem;
      box-sizing: border-box;
    }

    h1 {
      font-family: 'Fraunces', Georgia, serif;
      margin: 0 0 0.35rem;
      color: #0d4f52;
    }

    h2 {
      font-size: 1rem;
      margin: 0 0 0.5rem;
      color: #0d4f52;
    }

    .page-header p {
      margin: 0 0 1.25rem;
      color: var(--mat-sys-on-surface-variant);
    }

    .form {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }

    .full-width {
      width: 100%;
      max-width: 360px;
    }

    .field-row {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      gap: 0.75rem;
    }

    .currency-badge {
      font-size: 0.875rem;
      color: var(--mat-sys-on-surface-variant);
    }

    .currency-note,
    .eur-notice {
      padding: 0.75rem 1rem;
      background: rgba(13, 115, 119, 0.08);
      border-radius: 8px;
      margin: 0 0 1rem;
      line-height: 1.5;
    }

    mat-chip {
      cursor: pointer;
    }

    .unmapped ul {
      margin: 0.25rem 0 0;
      padding-left: 1.25rem;
      color: var(--mat-sys-on-surface-variant);
    }

    .actions button {
      margin-top: 1rem;
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
    }

    .error {
      color: var(--mat-sys-error);
    }
  `,
})
export class ConfirmPreferencesComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly state = inject(TripPlanningStateService);
  private readonly aiService = inject(AiService);
  private readonly recommendationService = inject(RecommendationService);
  private readonly interestService = inject(InterestService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  readonly tripTypes = SUPPORTED_TRIP_TYPES;
  readonly travelPeriods = TRAVEL_PERIODS;
  readonly climates = DEMO_CLIMATES;

  readonly interests = signal<Interest[]>([]);
  readonly interestsLoading = signal(true);
  readonly selectedInterestIds = signal<number[]>([]);
  readonly submitting = signal(false);
  readonly submitError = signal<string | null>(null);

  readonly unmappedPreferences = computed(
    () => this.state.confirmedPreferences()?.unmappedPreferences ?? []
  );

  readonly extractedCurrencyNote = computed(
    () => this.state.extractedPreferences()?.currencyNote ?? null
  );

  readonly displayCurrency = computed(() => {
    const confirmed = this.state.confirmedPreferences();
    return confirmed?.budgetCurrency && confirmed.budgetCurrency !== 'EUR'
      ? confirmed.budgetCurrency
      : null;
  });

  readonly requiresEurBudget = computed(() => {
    const confirmed = this.state.confirmedPreferences();
    if (!confirmed) {
      return false;
    }
    const hasBudget = confirmed.budget != null && confirmed.budget > 0;
    const currency = (confirmed.budgetCurrency ?? 'EUR').toUpperCase();
    return confirmed.requiresCurrencyConversion || (hasBudget && currency !== 'EUR');
  });

  readonly originalBudgetLabel = computed(() => {
    const confirmed = this.state.confirmedPreferences();
    if (!confirmed?.budget) {
      return null;
    }
    const currency = confirmed.budgetCurrency ?? 'EUR';
    return `${confirmed.budget} ${currency}`;
  });

  readonly form = this.fb.group({
    budget: this.fb.control<number | null>(null),
    duration: this.fb.control<number | null>(null, [Validators.min(1), Validators.max(60)]),
    tripType: this.fb.control<string | null>(null),
    travelPeriod: this.fb.control<string | null>(null),
    preferredClimate: this.fb.control<string | null>(null),
  });

  ngOnInit(): void {
    if (!this.state.hasExtraction()) {
      this.snackBar.open('No AI extraction found. Describe your trip first.', 'OK', {
        duration: 4000,
      });
      void this.router.navigate(['/plan-trip/ai']);
      return;
    }

    const confirmed = this.state.confirmedPreferences()!;
    const needsEur = this.requiresEurBudget();

    this.form.patchValue({
      budget: needsEur ? null : confirmed.budget,
      duration: confirmed.duration,
      tripType: confirmed.tripType,
      travelPeriod: confirmed.travelPeriod,
      preferredClimate: confirmed.preferredClimate,
    });

    if (needsEur) {
      this.form.controls.budget.setValidators([Validators.required, Validators.min(1)]);
    }

    this.selectedInterestIds.set([...confirmed.interestIds]);

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

  confirm(): void {
    if (this.submitting() || this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.submitError.set(null);

    const extracted = this.state.extractedPreferences();
    const raw = this.form.getRawValue();
    const budget = raw.budget != null && raw.budget > 0 ? raw.budget : null;

    this.state.updateConfirmed({
      budget,
      budgetCurrency: 'EUR',
      duration: raw.duration,
      tripType: raw.tripType,
      travelPeriod: raw.travelPeriod,
      preferredClimate: raw.preferredClimate,
      interestIds: this.selectedInterestIds(),
      unmappedPreferences: this.unmappedPreferences(),
      additionalRequirements: this.state.confirmedPreferences()?.additionalRequirements ?? null,
      requiresCurrencyConversion: false,
    });

    this.aiService
      .createPreferenceFromConfirmedData({
        budget,
        budgetCurrency: 'EUR',
        duration: raw.duration,
        tripType: raw.tripType,
        travelPeriod: raw.travelPeriod,
        preferredClimate: raw.preferredClimate,
        minTemperature: extracted?.minTemperature ?? null,
        maxTemperature: extracted?.maxTemperature ?? null,
        interestIds: this.selectedInterestIds(),
      })
      .pipe(
        switchMap((response) =>
          this.recommendationService.generateRecommendation(response.userPreferenceId, 5)
        )
      )
      .subscribe({
        next: (recommendation) => {
          this.state.setLastGenerated(recommendation);
          this.submitting.set(false);
          void this.router.navigate(['/recommendations', recommendation.id]);
        },
        error: (err) => {
          this.submitting.set(false);
          this.submitError.set(extractApiErrorMessage(err, 'Could not save preferences.'));
        },
      });
  }
}
