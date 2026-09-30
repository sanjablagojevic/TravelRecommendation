import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Category } from '../../../core/models/category.models';
import { CategoryService } from '../../../core/services/category.service';
import { extractApiErrorMessage } from '../../../core/utils/api-error.util';

export interface CategoryDialogData {
  category?: Category;
}

@Component({
  selector: 'app-category-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
  ],
  template: `
    <h2 mat-dialog-title>{{ isEdit ? 'Edit category' : 'Add category' }}</h2>
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
          <mat-label>Type</mat-label>
          <input matInput formControlName="type" maxlength="100" required />
          <mat-hint>For example Nature, City, Adventure.</mat-hint>
          @if (form.controls.type.hasError('required')) {
            <mat-error>Type is required.</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline" class="full">
          <mat-label>Description</mat-label>
          <textarea matInput rows="3" formControlName="description" maxlength="500"></textarea>
        </mat-form-field>

        @if (saveError()) {
          <p class="error">{{ saveError() }}</p>
        }
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
        {{ isEdit ? 'Save changes' : 'Add category' }}
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

    .full {
      width: 100%;
    }

    .error {
      margin: 0;
      color: var(--mat-sys-error);
      font-size: 0.875rem;
    }
  `,
})
export class CategoryDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly categoryService = inject(CategoryService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialogRef =
    inject<MatDialogRef<CategoryDialogComponent, Category>>(MatDialogRef);
  private readonly data = inject<CategoryDialogData>(MAT_DIALOG_DATA);

  readonly isEdit = this.data.category != null;
  readonly saving = signal(false);
  readonly saveError = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    name: [this.data.category?.name ?? '', [Validators.required, Validators.maxLength(150)]],
    type: [this.data.category?.type ?? '', [Validators.required, Validators.maxLength(100)]],
    description: [this.data.category?.description ?? '', [Validators.maxLength(500)]],
  });

  save(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const payload = {
      name: value.name.trim(),
      type: value.type.trim(),
      description: value.description.trim() || null,
    };

    this.saving.set(true);
    this.saveError.set(null);

    const existing = this.data.category;
    const request$ = existing
      ? this.categoryService.update(existing.id, payload)
      : this.categoryService.create(payload);

    request$.subscribe({
      next: (category) => {
        this.saving.set(false);
        this.snackBar.open(existing ? 'Category updated' : 'Category created', 'Dismiss', {
          duration: 3000,
        });
        this.dialogRef.close(category);
      },
      error: (err) => {
        this.saving.set(false);
        this.saveError.set(extractApiErrorMessage(err, 'Unable to save this category.'));
      },
    });
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
