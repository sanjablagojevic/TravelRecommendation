import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Activity } from '../../../core/models/destination.models';
import { ActivityService } from '../../../core/services/activity.service';
import { extractApiErrorMessage } from '../../../core/utils/api-error.util';

export interface ActivityDialogData {
  destinationId: number;
  activity?: Activity;
}

@Component({
  selector: 'app-activity-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
  ],
  template: `
    <h2 mat-dialog-title>{{ isEdit ? 'Edit activity' : 'Add activity' }}</h2>
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
          <mat-label>Provider</mat-label>
          <input matInput formControlName="provider" maxlength="150" />
          <mat-hint>Free text, for example Local, Local Demo, or Tour Operator.</mat-hint>
        </mat-form-field>

        <div class="row">
          <mat-form-field appearance="outline">
            <mat-label>Price</mat-label>
            <input matInput type="number" min="0" step="0.01" formControlName="price" />
            @if (form.controls.price.hasError('min')) {
              <mat-error>Price must be 0 or greater.</mat-error>
            }
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Currency</mat-label>
            <input matInput formControlName="currency" maxlength="10" placeholder="EUR" />
          </mat-form-field>
        </div>

        <mat-form-field appearance="outline" class="full">
          <mat-label>External URL</mat-label>
          <input matInput formControlName="externalUrl" maxlength="1000" />
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
        {{ isEdit ? 'Save changes' : 'Add activity' }}
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
  `,
})
export class ActivityDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly activityService = inject(ActivityService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialogRef =
    inject<MatDialogRef<ActivityDialogComponent, Activity>>(MatDialogRef);
  private readonly data = inject<ActivityDialogData>(MAT_DIALOG_DATA);

  readonly isEdit = this.data.activity != null;
  readonly saving = signal(false);

  readonly form = this.fb.group({
    name: this.fb.nonNullable.control(this.data.activity?.name ?? '', [
      Validators.required,
      Validators.maxLength(150),
    ]),
    provider: this.fb.nonNullable.control(this.data.activity?.provider ?? '', [
      Validators.maxLength(150),
    ]),
    price: this.fb.control<number | null>(this.data.activity?.price ?? null, [Validators.min(0)]),
    currency: this.fb.nonNullable.control(this.data.activity?.currency ?? '', [
      Validators.maxLength(10),
    ]),
    externalUrl: this.fb.nonNullable.control(this.data.activity?.externalUrl ?? '', [
      Validators.maxLength(1000),
    ]),
    description: this.fb.nonNullable.control(this.data.activity?.description ?? ''),
  });

  save(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const payload = {
      name: value.name.trim(),
      provider: value.provider.trim() || null,
      price: value.price ?? null,
      currency: value.currency.trim() || null,
      externalUrl: value.externalUrl.trim() || null,
      description: value.description.trim() || null,
    };

    this.saving.set(true);
    const existing = this.data.activity;
    const request$ = existing
      ? this.activityService.update(existing.id, payload)
      : this.activityService.create({ ...payload, destinationId: this.data.destinationId });

    request$.subscribe({
      next: (activity) => {
        this.saving.set(false);
        this.snackBar.open(existing ? 'Activity updated' : 'Activity added', 'Dismiss', {
          duration: 3000,
        });
        this.dialogRef.close(activity);
      },
      error: (err) => {
        this.saving.set(false);
        this.snackBar.open(extractApiErrorMessage(err, 'Unable to save this activity.'), 'Dismiss', {
          duration: 5000,
        });
      },
    });
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
