import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import {
  MAX_ITINERARY_DAYS,
  MIN_ITINERARY_DAYS,
} from '../../core/models/itinerary.models';
import { ItineraryService } from '../../core/services/itinerary.service';
import { extractApiErrorMessage } from '../../core/utils/api-error.util';

export interface GenerateItineraryDialogData {
  destinationId: number;
  destinationName: string;
  userPreferenceId?: number | null;
  recommendationId?: number | null;
  defaultDurationDays?: number | null;
}

@Component({
  selector: 'app-generate-itinerary-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <h2 mat-dialog-title>Plan this trip</h2>
    <mat-dialog-content>
      <p class="intro">
        We build a day-by-day plan from the attractions and activities in our catalog for this
        destination.
      </p>

      <form class="dialog-form" [formGroup]="form" (ngSubmit)="generate()">
        <mat-form-field appearance="outline" class="full">
          <mat-label>Destination</mat-label>
          <input matInput [value]="data.destinationName" readonly />
        </mat-form-field>

        <mat-form-field appearance="outline" class="full">
          <mat-label>Trip length (days)</mat-label>
          <input
            matInput
            type="number"
            formControlName="durationDays"
            [min]="minDays"
            [max]="maxDays"
            required
          />
          <mat-hint>Between {{ minDays }} and {{ maxDays }} days.</mat-hint>
          @if (form.controls.durationDays.hasError('required')) {
            <mat-error>Trip length is required.</mat-error>
          }
          @if (
            form.controls.durationDays.hasError('min') || form.controls.durationDays.hasError('max')
          ) {
            <mat-error>Choose between {{ minDays }} and {{ maxDays }} days.</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline" class="full">
          <mat-label>Anything else we should consider?</mat-label>
          <textarea
            matInput
            rows="3"
            formControlName="additionalRequest"
            maxlength="1000"
          ></textarea>
          <mat-hint>Optional, up to 1000 characters.</mat-hint>
          @if (form.controls.additionalRequest.hasError('maxlength')) {
            <mat-error>Please keep this under 1000 characters.</mat-error>
          }
        </mat-form-field>

        @if (error()) {
          <p class="error">{{ error() }}</p>
        }
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" [disabled]="submitting()" (click)="cancel()">Cancel</button>
      <button
        mat-flat-button
        color="primary"
        type="button"
        class="generate-btn"
        [disabled]="form.invalid || submitting()"
        (click)="generate()"
      >
        @if (submitting()) {
          <mat-spinner diameter="18" />
          Creating your travel plan...
        } @else {
          Generate itinerary
        }
      </button>
    </mat-dialog-actions>
  `,
  styles: `
    h2 {
      font-family: Fraunces, Georgia, serif;
      color: #0d4f52;
    }

    .intro {
      margin: 0 0 0.75rem;
      color: var(--mat-sys-on-surface-variant);
      line-height: 1.5;
    }

    .dialog-form {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
      min-width: min(420px, 100%);
    }

    .full {
      width: 100%;
    }

    .generate-btn {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
    }

    .error {
      margin: 0;
      color: var(--mat-sys-error);
      font-size: 0.875rem;
    }
  `,
})
export class GenerateItineraryDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly itineraryService = inject(ItineraryService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);
  private readonly dialogRef =
    inject<MatDialogRef<GenerateItineraryDialogComponent, number>>(MatDialogRef);
  readonly data = inject<GenerateItineraryDialogData>(MAT_DIALOG_DATA);

  readonly minDays = MIN_ITINERARY_DAYS;
  readonly maxDays = MAX_ITINERARY_DAYS;
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    durationDays: [
      this.clampDuration(this.data.defaultDurationDays),
      [Validators.required, Validators.min(MIN_ITINERARY_DAYS), Validators.max(MAX_ITINERARY_DAYS)],
    ],
    additionalRequest: ['', [Validators.maxLength(1000)]],
  });

  generate(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.submitting.set(true);
    this.error.set(null);
    this.form.disable();

    this.itineraryService
      .generate({
        destinationId: this.data.destinationId,
        durationDays: Number(value.durationDays),
        userPreferenceId: this.data.userPreferenceId ?? null,
        recommendationId: this.data.recommendationId ?? null,
        additionalRequest: value.additionalRequest.trim() || null,
      })
      .subscribe({
        next: (itinerary) => {
          this.submitting.set(false);
          this.snackBar.open('Your itinerary is ready.', 'Dismiss', { duration: 4000 });
          this.dialogRef.close(itinerary.id);
          void this.router.navigate(['/itineraries', itinerary.id]);
        },
        error: (err) => {
          this.submitting.set(false);
          this.form.enable();
          this.error.set(this.describeError(err));
        },
      });
  }

  cancel(): void {
    this.dialogRef.close();
  }

  /** The API returns 503 when the AI provider is unreachable or returns unusable output. */
  private describeError(err: unknown): string {
    const unavailable = 'Itinerary generation is temporarily unavailable. Please try again later.';
    if (err instanceof HttpErrorResponse && err.status === 503) {
      return unavailable;
    }
    return extractApiErrorMessage(err, unavailable);
  }

  private clampDuration(value: number | null | undefined): number {
    if (value == null || !Number.isFinite(value)) {
      return 3;
    }
    return Math.min(MAX_ITINERARY_DAYS, Math.max(MIN_ITINERARY_DAYS, Math.round(value)));
  }
}
