import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Attraction } from '../../../core/models/destination.models';
import { AttractionService } from '../../../core/services/attraction.service';
import { extractApiErrorMessage } from '../../../core/utils/api-error.util';

export interface AttractionDialogData {
  destinationId: number;
  attraction?: Attraction;
}

@Component({
  selector: 'app-attraction-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
  ],
  template: `
    <h2 mat-dialog-title>{{ isEdit ? 'Edit attraction' : 'Add attraction' }}</h2>
    <mat-dialog-content>
      <form class="dialog-form" [formGroup]="form" (ngSubmit)="save()">
        <mat-form-field appearance="outline" class="full">
          <mat-label>Name</mat-label>
          <input matInput formControlName="name" maxlength="150" required />
          @if (form.controls.name.hasError('required')) {
            <mat-error>Name is required.</mat-error>
          }
          @if (form.controls.name.hasError('maxlength')) {
            <mat-error>Name must be at most 150 characters.</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline" class="full">
          <mat-label>Location</mat-label>
          <input matInput formControlName="location" maxlength="250" />
          <mat-hint>Optional address or area inside the destination.</mat-hint>
        </mat-form-field>

        <div class="row">
          <mat-form-field appearance="outline">
            <mat-label>Latitude</mat-label>
            <input matInput type="number" step="0.000001" formControlName="latitude" />
            @if (form.controls.latitude.hasError('min') || form.controls.latitude.hasError('max')) {
              <mat-error>Latitude must be between -90 and 90.</mat-error>
            }
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Longitude</mat-label>
            <input matInput type="number" step="0.000001" formControlName="longitude" />
            @if (
              form.controls.longitude.hasError('min') || form.controls.longitude.hasError('max')
            ) {
              <mat-error>Longitude must be between -180 and 180.</mat-error>
            }
          </mat-form-field>
        </div>
        <p class="helper">
          Coordinates are used to display this attraction on the destination map.
        </p>

        <mat-form-field appearance="outline" class="full">
          <mat-label>Image URL</mat-label>
          <input matInput formControlName="imageUrl" maxlength="1000" />
        </mat-form-field>

        <mat-form-field appearance="outline" class="full">
          <mat-label>Description</mat-label>
          <textarea matInput rows="3" formControlName="description"></textarea>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" [disabled]="saving()" (click)="cancel()">Cancel</button>
      <button
        mat-flat-button
        color="primary"
        type="button"
        [disabled]="form.invalid || saving()"
        (click)="save()"
      >
        {{ isEdit ? 'Save changes' : 'Add attraction' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: `
    h2 {
      font-family: Fraunces, Georgia, serif;
      color: #0d4f52;
    }

    .dialog-form {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
      padding-top: 0.5rem;
    }

    .row {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
      gap: 0 1rem;
    }

    .full {
      width: 100%;
    }

    .helper {
      margin: 0 0 0.75rem;
      font-size: 0.8rem;
      color: var(--mat-sys-on-surface-variant);
    }
  `,
})
export class AttractionDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly attractionService = inject(AttractionService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialogRef =
    inject<MatDialogRef<AttractionDialogComponent, Attraction>>(MatDialogRef);
  private readonly data = inject<AttractionDialogData>(MAT_DIALOG_DATA);

  readonly isEdit = this.data.attraction != null;
  readonly saving = signal(false);

  readonly form = this.fb.group({
    name: this.fb.nonNullable.control(this.data.attraction?.name ?? '', [
      Validators.required,
      Validators.maxLength(150),
    ]),
    location: this.fb.nonNullable.control(this.data.attraction?.location ?? '', [
      Validators.maxLength(250),
    ]),
    latitude: this.fb.control<number | null>(this.data.attraction?.latitude ?? null, [
      Validators.min(-90),
      Validators.max(90),
    ]),
    longitude: this.fb.control<number | null>(this.data.attraction?.longitude ?? null, [
      Validators.min(-180),
      Validators.max(180),
    ]),
    imageUrl: this.fb.nonNullable.control(this.data.attraction?.imageUrl ?? '', [
      Validators.maxLength(1000),
    ]),
    description: this.fb.nonNullable.control(this.data.attraction?.description ?? ''),
  });

  save(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const payload = {
      name: value.name.trim(),
      location: value.location.trim() || null,
      latitude: value.latitude ?? null,
      longitude: value.longitude ?? null,
      imageUrl: value.imageUrl.trim() || null,
      description: value.description.trim() || null,
    };

    this.saving.set(true);
    const existing = this.data.attraction;
    const request$ = existing
      ? this.attractionService.update(existing.id, payload)
      : this.attractionService.create({ ...payload, destinationId: this.data.destinationId });

    request$.subscribe({
      next: (attraction) => {
        this.saving.set(false);
        this.snackBar.open(existing ? 'Attraction updated' : 'Attraction added', 'Dismiss', {
          duration: 3000,
        });
        this.dialogRef.close(attraction);
      },
      error: (err) => {
        this.saving.set(false);
        this.snackBar.open(
          extractApiErrorMessage(err, 'Unable to save this attraction.'),
          'Dismiss',
          { duration: 5000 }
        );
      },
    });
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
