import { CurrencyPipe } from '@angular/common';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin, of, switchMap } from 'rxjs';
import { Category, Interest } from '../../../core/models/category.models';
import {
  Activity,
  Attraction,
  CreateDestinationRequest,
  DestinationDetails,
} from '../../../core/models/destination.models';
import { ActivityService } from '../../../core/services/activity.service';
import { AttractionService } from '../../../core/services/attraction.service';
import { CategoryService } from '../../../core/services/category.service';
import { DestinationService } from '../../../core/services/destination.service';
import { InterestService } from '../../../core/services/interest.service';
import { extractApiErrorMessage } from '../../../core/utils/api-error.util';
import { UnsavedChangesAware } from '../../../core/guards/unsaved-changes.guard';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { ActivityDialogComponent, ActivityDialogData } from './activity-dialog.component';
import { AttractionDialogComponent, AttractionDialogData } from './attraction-dialog.component';

@Component({
  selector: 'app-admin-destination-form',
  imports: [
    CurrencyPipe,
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatTabsModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    LoadingSpinnerComponent,
  ],
  template: `
    <header class="page-header">
      <a mat-button routerLink="/admin/destinations">
        <mat-icon>arrow_back</mat-icon>
        Back to destinations
      </a>
      <h1>{{ isEdit() ? 'Edit destination' : 'New destination' }}</h1>
      <p>
        {{
          isEdit()
            ? 'Update the listing, its taxonomy, attractions, and activities.'
            : 'Add a new destination to the catalog.'
        }}
      </p>
    </header>

    @if (loading()) {
      <app-loading-spinner message="Loading destination..." />
    } @else if (loadError()) {
      <mat-card appearance="outlined" class="message-card">
        <mat-card-content>
          <p class="error">{{ loadError() }}</p>
          <a mat-flat-button routerLink="/admin/destinations">Back to destinations</a>
        </mat-card-content>
      </mat-card>
    } @else {
      <form [formGroup]="form" (ngSubmit)="submit()">
        <mat-tab-group>
          <mat-tab label="General">
            <div class="tab-body">
              <div class="grid">
                <mat-form-field appearance="outline">
                  <mat-label>Name</mat-label>
                  <input matInput formControlName="name" maxlength="150" required />
                  @if (form.controls.name.hasError('required')) {
                    <mat-error>Name is required.</mat-error>
                  }
                  @if (form.controls.name.hasError('maxlength')) {
                    <mat-error>Name must be at most 150 characters.</mat-error>
                  }
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Country</mat-label>
                  <input matInput formControlName="country" maxlength="100" required />
                  @if (form.controls.country.hasError('required')) {
                    <mat-error>Country is required.</mat-error>
                  }
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>City</mat-label>
                  <input matInput formControlName="city" maxlength="100" required />
                  @if (form.controls.city.hasError('required')) {
                    <mat-error>City is required.</mat-error>
                  }
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Climate</mat-label>
                  <input matInput formControlName="climate" maxlength="100" />
                  <mat-hint>For example Mediterranean, Tropical, Continental.</mat-hint>
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Average daily cost</mat-label>
                  <input
                    matInput
                    type="number"
                    min="0"
                    step="1"
                    formControlName="averageDailyCost"
                    required
                  />
                  @if (form.controls.averageDailyCost.hasError('required')) {
                    <mat-error>Average daily cost is required.</mat-error>
                  }
                  @if (form.controls.averageDailyCost.hasError('min')) {
                    <mat-error>Average daily cost must be 0 or greater.</mat-error>
                  }
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Popularity</mat-label>
                  <input
                    matInput
                    type="number"
                    min="0"
                    max="1"
                    step="0.01"
                    formControlName="popularity"
                    required
                  />
                  <mat-hint>Between 0 and 1, in steps of 0.01 (for example 0.85).</mat-hint>
                  @if (form.controls.popularity.hasError('required')) {
                    <mat-error>Popularity is required.</mat-error>
                  }
                  @if (
                    form.controls.popularity.hasError('min') ||
                    form.controls.popularity.hasError('max')
                  ) {
                    <mat-error>Popularity must be between 0 and 1.</mat-error>
                  }
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Latitude</mat-label>
                  <input
                    matInput
                    type="number"
                    step="0.000001"
                    formControlName="latitude"
                    required
                  />
                  @if (form.controls.latitude.hasError('required')) {
                    <mat-error>Latitude is required.</mat-error>
                  }
                  @if (
                    form.controls.latitude.hasError('min') || form.controls.latitude.hasError('max')
                  ) {
                    <mat-error>Latitude must be between -90 and 90.</mat-error>
                  }
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Longitude</mat-label>
                  <input
                    matInput
                    type="number"
                    step="0.000001"
                    formControlName="longitude"
                    required
                  />
                  @if (form.controls.longitude.hasError('required')) {
                    <mat-error>Longitude is required.</mat-error>
                  }
                  @if (
                    form.controls.longitude.hasError('min') ||
                    form.controls.longitude.hasError('max')
                  ) {
                    <mat-error>Longitude must be between -180 and 180.</mat-error>
                  }
                </mat-form-field>
              </div>

              <mat-form-field appearance="outline" class="full">
                <mat-label>Description</mat-label>
                <textarea matInput rows="5" formControlName="description"></textarea>
              </mat-form-field>

              <mat-form-field appearance="outline" class="full">
                <mat-label>Image URL</mat-label>
                <input matInput formControlName="imageUrl" maxlength="1000" />
                <mat-hint>Optional. Leave empty to use the placeholder image.</mat-hint>
              </mat-form-field>

              <div class="preview">
                <img
                  [src]="imagePreview() && !imageFailed() ? imagePreview() : placeholder"
                  alt="Destination image preview"
                  (error)="imageFailed.set(true)"
                />
                <span class="muted">
                  @if (imagePreview() && imageFailed()) {
                    Image could not be loaded. Showing the placeholder instead.
                  } @else {
                    Image preview
                  }
                </span>
              </div>

              <mat-slide-toggle formControlName="isActive">
                Active (visible to travellers)
              </mat-slide-toggle>
            </div>
          </mat-tab>

          <mat-tab label="Categories & Interests">
            <div class="tab-body">
              <mat-form-field appearance="outline" class="full">
                <mat-label>Categories</mat-label>
                <mat-select formControlName="categoryIds" multiple>
                  @for (category of categories(); track category.id) {
                    <mat-option [value]="category.id">
                      {{ category.name }} ({{ category.type }})
                    </mat-option>
                  }
                </mat-select>
                <mat-hint>Categories group destinations on the public catalog.</mat-hint>
              </mat-form-field>

              <mat-form-field appearance="outline" class="full">
                <mat-label>Interests</mat-label>
                <mat-select formControlName="interestIds" multiple>
                  @for (interest of interests(); track interest.id) {
                    <mat-option [value]="interest.id">{{ interest.name }}</mat-option>
                  }
                </mat-select>
                <mat-hint>
                  Interests are used by the recommendation engine to match user preferences with
                  destinations.
                </mat-hint>
              </mat-form-field>
            </div>
          </mat-tab>

          @if (isEdit()) {
            <mat-tab label="Attractions">
              <div class="tab-body">
                <div class="section-header">
                  <h2>Attractions</h2>
                  <button mat-flat-button color="primary" type="button" (click)="openAttraction()">
                    <mat-icon>add</mat-icon>
                    Add attraction
                  </button>
                </div>
                <p class="muted">
                  Coordinates are used to display this attraction on the destination map.
                </p>

                @if (!attractions().length) {
                  <p class="empty">No attractions yet.</p>
                } @else {
                  <div class="table-scroll">
                    <table mat-table [dataSource]="attractions()">
                      <ng-container matColumnDef="name">
                        <th mat-header-cell *matHeaderCellDef>Name</th>
                        <td mat-cell *matCellDef="let row">
                          <span class="strong">{{ row.name }}</span>
                        </td>
                      </ng-container>
                      <ng-container matColumnDef="location">
                        <th mat-header-cell *matHeaderCellDef>Location</th>
                        <td mat-cell *matCellDef="let row">{{ row.location || '—' }}</td>
                      </ng-container>
                      <ng-container matColumnDef="coordinates">
                        <th mat-header-cell *matHeaderCellDef>Coordinates</th>
                        <td mat-cell *matCellDef="let row">
                          {{ formatCoordinates(row.latitude, row.longitude) }}
                        </td>
                      </ng-container>
                      <ng-container matColumnDef="actions">
                        <th mat-header-cell *matHeaderCellDef class="actions-header">Actions</th>
                        <td mat-cell *matCellDef="let row" class="actions-cell">
                          <button
                            mat-icon-button
                            type="button"
                            matTooltip="Edit attraction"
                            aria-label="Edit attraction"
                            (click)="openAttraction(row)"
                          >
                            <mat-icon>edit</mat-icon>
                          </button>
                          <button
                            mat-icon-button
                            type="button"
                            matTooltip="Delete attraction"
                            aria-label="Delete attraction"
                            (click)="confirmDeleteAttraction(row)"
                          >
                            <mat-icon>delete</mat-icon>
                          </button>
                        </td>
                      </ng-container>
                      <tr mat-header-row *matHeaderRowDef="attractionColumns"></tr>
                      <tr mat-row *matRowDef="let row; columns: attractionColumns"></tr>
                    </table>
                  </div>
                }
              </div>
            </mat-tab>

            <mat-tab label="Activities">
              <div class="tab-body">
                <div class="section-header">
                  <h2>Activities</h2>
                  <button mat-flat-button color="primary" type="button" (click)="openActivity()">
                    <mat-icon>add</mat-icon>
                    Add activity
                  </button>
                </div>
                <p class="muted">
                  Providers are free text, for example Local, Local Demo, or Tour Operator.
                </p>

                @if (!activities().length) {
                  <p class="empty">No activities yet.</p>
                } @else {
                  <div class="table-scroll">
                    <table mat-table [dataSource]="activities()">
                      <ng-container matColumnDef="name">
                        <th mat-header-cell *matHeaderCellDef>Name</th>
                        <td mat-cell *matCellDef="let row">
                          <span class="strong">{{ row.name }}</span>
                        </td>
                      </ng-container>
                      <ng-container matColumnDef="provider">
                        <th mat-header-cell *matHeaderCellDef>Provider</th>
                        <td mat-cell *matCellDef="let row">{{ row.provider || '—' }}</td>
                      </ng-container>
                      <ng-container matColumnDef="price">
                        <th mat-header-cell *matHeaderCellDef>Price</th>
                        <td mat-cell *matCellDef="let row">
                          @if (row.price != null) {
                            {{ row.price | currency: row.currency || 'EUR' : 'symbol' : '1.0-2' }}
                          } @else {
                            —
                          }
                        </td>
                      </ng-container>
                      <ng-container matColumnDef="externalUrl">
                        <th mat-header-cell *matHeaderCellDef>Link</th>
                        <td mat-cell *matCellDef="let row">
                          @if (row.externalUrl) {
                            <a [href]="row.externalUrl" target="_blank" rel="noopener noreferrer">
                              Open
                            </a>
                          } @else {
                            —
                          }
                        </td>
                      </ng-container>
                      <ng-container matColumnDef="actions">
                        <th mat-header-cell *matHeaderCellDef class="actions-header">Actions</th>
                        <td mat-cell *matCellDef="let row" class="actions-cell">
                          <button
                            mat-icon-button
                            type="button"
                            matTooltip="Edit activity"
                            aria-label="Edit activity"
                            (click)="openActivity(row)"
                          >
                            <mat-icon>edit</mat-icon>
                          </button>
                          <button
                            mat-icon-button
                            type="button"
                            matTooltip="Delete activity"
                            aria-label="Delete activity"
                            (click)="confirmDeleteActivity(row)"
                          >
                            <mat-icon>delete</mat-icon>
                          </button>
                        </td>
                      </ng-container>
                      <tr mat-header-row *matHeaderRowDef="activityColumns"></tr>
                      <tr mat-row *matRowDef="let row; columns: activityColumns"></tr>
                    </table>
                  </div>
                }
              </div>
            </mat-tab>
          }
        </mat-tab-group>

        <div class="form-actions">
          <button mat-flat-button color="primary" type="submit" [disabled]="saving()">
            {{ isEdit() ? 'Save changes' : 'Create destination' }}
          </button>
          <a mat-button routerLink="/admin/destinations">Cancel</a>
        </div>
      </form>
    }
  `,
  styles: `
    :host {
      display: block;
      width: min(1100px, 100%);
      margin: 0 auto;
      padding: 1.5rem 1rem 2rem;
      box-sizing: border-box;
    }

    .page-header h1 {
      font-family: Fraunces, Georgia, serif;
      margin: 0.5rem 0 0.25rem;
      color: #0d4f52;
    }

    .page-header p {
      margin: 0 0 1rem;
      color: var(--mat-sys-on-surface-variant);
    }

    .tab-body {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      padding: 1.25rem 0.25rem;
    }

    .grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: 0 1rem;
    }

    .full {
      width: 100%;
    }

    .preview {
      display: flex;
      align-items: center;
      gap: 1rem;
      margin-bottom: 0.75rem;
    }

    .preview img {
      width: 160px;
      height: 100px;
      object-fit: cover;
      border-radius: 8px;
      background: #e0f7fa;
    }

    .section-header {
      display: flex;
      flex-wrap: wrap;
      gap: 0.75rem;
      align-items: center;
      justify-content: space-between;
    }

    .section-header h2 {
      margin: 0;
      font-size: 1.1rem;
      font-family: Fraunces, Georgia, serif;
      color: #0d4f52;
    }

    .table-scroll {
      overflow-x: auto;
      border: 1px solid var(--mat-sys-outline-variant);
      border-radius: 12px;
    }

    table {
      width: 100%;
      min-width: 640px;
    }

    .strong {
      font-weight: 600;
    }

    .actions-header,
    .actions-cell {
      text-align: right;
      white-space: nowrap;
    }

    .form-actions {
      display: flex;
      gap: 0.75rem;
      align-items: center;
      margin-top: 1.5rem;
    }

    .muted,
    .empty {
      margin: 0;
      color: var(--mat-sys-on-surface-variant);
      font-size: 0.875rem;
    }

    .error {
      margin: 0 0 1rem;
      color: var(--mat-sys-error);
    }

    .message-card {
      max-width: 520px;
    }
  `,
})
export class AdminDestinationFormComponent implements UnsavedChangesAware {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);
  private readonly destinationService = inject(DestinationService);
  private readonly categoryService = inject(CategoryService);
  private readonly interestService = inject(InterestService);
  private readonly attractionService = inject(AttractionService);
  private readonly activityService = inject(ActivityService);

  readonly placeholder = '/assets/images/destination-placeholder.svg';
  readonly attractionColumns = ['name', 'location', 'coordinates', 'actions'];
  readonly activityColumns = ['name', 'provider', 'price', 'externalUrl', 'actions'];

  readonly destinationId = signal<number | null>(null);
  readonly categories = signal<Category[]>([]);
  readonly interests = signal<Interest[]>([]);
  readonly attractions = signal<Attraction[]>([]);
  readonly activities = signal<Activity[]>([]);
  readonly loading = signal(true);
  readonly loadError = signal<string | null>(null);
  readonly saving = signal(false);
  readonly imagePreview = signal('');
  readonly imageFailed = signal(false);

  readonly form = this.fb.group({
    name: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(150)]),
    country: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(100)]),
    city: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(100)]),
    description: this.fb.nonNullable.control(''),
    averageDailyCost: this.fb.control<number | null>(null, [
      Validators.required,
      Validators.min(0),
    ]),
    latitude: this.fb.control<number | null>(null, [
      Validators.required,
      Validators.min(-90),
      Validators.max(90),
    ]),
    longitude: this.fb.control<number | null>(null, [
      Validators.required,
      Validators.min(-180),
      Validators.max(180),
    ]),
    popularity: this.fb.control<number | null>(0.5, [
      Validators.required,
      Validators.min(0),
      Validators.max(1),
    ]),
    imageUrl: this.fb.nonNullable.control('', [Validators.maxLength(1000)]),
    climate: this.fb.nonNullable.control('', [Validators.maxLength(100)]),
    isActive: this.fb.nonNullable.control(true),
    categoryIds: this.fb.nonNullable.control<number[]>([]),
    interestIds: this.fb.nonNullable.control<number[]>([]),
  });

  readonly isEdit = computed(() => this.destinationId() != null);

  constructor() {
    this.form.controls.imageUrl.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((value) => {
        this.imagePreview.set(value.trim());
        this.imageFailed.set(false);
      });

    this.route.paramMap
      .pipe(
        switchMap((params) => {
          this.loading.set(true);
          this.loadError.set(null);

          const rawId = params.get('id');
          const id = rawId == null ? null : Number(rawId);
          this.destinationId.set(id != null && Number.isFinite(id) ? id : null);

          return forkJoin({
            categories: this.categoryService.getAll(),
            interests: this.interestService.getAll(),
            destination: this.destinationId()
              ? this.destinationService.getById(this.destinationId()!)
              : of(null),
          });
        }),
        takeUntilDestroyed()
      )
      .subscribe({
        next: ({ categories, interests, destination }) => {
          this.categories.set(categories);
          this.interests.set(interests);
          if (destination) {
            this.patchForm(destination);
          }
          this.loading.set(false);
        },
        error: (err) => {
          this.loadError.set(extractApiErrorMessage(err, 'Unable to load this destination.'));
          this.loading.set(false);
        },
      });
  }

  hasUnsavedChanges(): boolean {
    return this.form.dirty && !this.saving();
  }

  formatCoordinates(latitude?: number | null, longitude?: number | null): string {
    if (latitude == null || longitude == null) {
      return '—';
    }
    return `${latitude}, ${longitude}`;
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.snackBar.open('Please fix the highlighted fields.', 'Dismiss', { duration: 4000 });
      return;
    }

    const value = this.form.getRawValue();
    const payload: CreateDestinationRequest = {
      name: value.name.trim(),
      country: value.country.trim(),
      city: value.city.trim(),
      description: value.description.trim() || null,
      averageDailyCost: value.averageDailyCost ?? 0,
      latitude: value.latitude ?? 0,
      longitude: value.longitude ?? 0,
      popularity: value.popularity ?? 0,
      imageUrl: value.imageUrl.trim() || null,
      climate: value.climate.trim() || null,
      isActive: value.isActive,
      categoryIds: value.categoryIds,
      interestIds: value.interestIds,
    };

    this.saving.set(true);
    const id = this.destinationId();
    const request$ = id
      ? this.destinationService.update(id, payload)
      : this.destinationService.create(payload);

    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.form.markAsPristine();
        this.snackBar.open(
          id ? 'Destination updated successfully.' : 'Destination created successfully.',
          'Dismiss',
          { duration: 3000 }
        );
        void this.router.navigate(['/admin/destinations']);
      },
      error: (err) => {
        this.saving.set(false);
        this.snackBar.open(
          extractApiErrorMessage(err, 'Unable to save this destination.'),
          'Dismiss',
          { duration: 5000 }
        );
      },
    });
  }

  openAttraction(attraction?: Attraction): void {
    const id = this.destinationId();
    if (!id) {
      return;
    }

    const data: AttractionDialogData = { destinationId: id, attraction };
    this.dialog
      .open<AttractionDialogComponent, AttractionDialogData, Attraction>(
        AttractionDialogComponent,
        { data, width: '560px' }
      )
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((saved) => {
        if (saved) {
          this.reloadAttractions();
        }
      });
  }

  confirmDeleteAttraction(attraction: Attraction): void {
    const data: ConfirmDialogData = {
      title: 'Delete attraction',
      message: `"${attraction.name}" will be permanently removed from this destination.`,
      confirmText: 'Delete',
      confirmColor: 'warn',
    };

    this.dialog
      .open<ConfirmDialogComponent, ConfirmDialogData, boolean>(ConfirmDialogComponent, {
        data,
        width: '420px',
      })
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.attractionService.delete(attraction.id).subscribe({
          next: () => {
            this.snackBar.open('Attraction deleted', 'Dismiss', { duration: 3000 });
            this.reloadAttractions();
          },
          error: (err) => {
            this.snackBar.open(
              extractApiErrorMessage(err, 'Unable to delete this attraction.'),
              'Dismiss',
              { duration: 5000 }
            );
          },
        });
      });
  }

  openActivity(activity?: Activity): void {
    const id = this.destinationId();
    if (!id) {
      return;
    }

    const data: ActivityDialogData = { destinationId: id, activity };
    this.dialog
      .open<ActivityDialogComponent, ActivityDialogData, Activity>(ActivityDialogComponent, {
        data,
        width: '560px',
      })
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((saved) => {
        if (saved) {
          this.reloadActivities();
        }
      });
  }

  confirmDeleteActivity(activity: Activity): void {
    const data: ConfirmDialogData = {
      title: 'Delete activity',
      message: `"${activity.name}" will be permanently removed from this destination.`,
      confirmText: 'Delete',
      confirmColor: 'warn',
    };

    this.dialog
      .open<ConfirmDialogComponent, ConfirmDialogData, boolean>(ConfirmDialogComponent, {
        data,
        width: '420px',
      })
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.activityService.delete(activity.id).subscribe({
          next: () => {
            this.snackBar.open('Activity deleted', 'Dismiss', { duration: 3000 });
            this.reloadActivities();
          },
          error: (err) => {
            this.snackBar.open(
              extractApiErrorMessage(err, 'Unable to delete this activity.'),
              'Dismiss',
              { duration: 5000 }
            );
          },
        });
      });
  }

  private patchForm(destination: DestinationDetails): void {
    this.form.patchValue({
      name: destination.name,
      country: destination.country,
      city: destination.city,
      description: destination.description ?? '',
      averageDailyCost: destination.averageDailyCost,
      latitude: destination.latitude,
      longitude: destination.longitude,
      popularity: destination.popularity,
      imageUrl: destination.imageUrl ?? '',
      climate: destination.climate ?? '',
      isActive: destination.isActive,
      categoryIds: destination.categories.map((category) => category.id),
      interestIds: destination.interests.map((interest) => interest.id),
    });
    this.form.markAsPristine();
    this.attractions.set(destination.attractions);
    this.activities.set(destination.activities);
  }

  private reloadAttractions(): void {
    const id = this.destinationId();
    if (!id) {
      return;
    }

    this.attractionService.getByDestination(id).subscribe({
      next: (items) => this.attractions.set(items),
      error: (err) => {
        this.snackBar.open(
          extractApiErrorMessage(err, 'Unable to refresh attractions.'),
          'Dismiss',
          { duration: 5000 }
        );
      },
    });
  }

  private reloadActivities(): void {
    const id = this.destinationId();
    if (!id) {
      return;
    }

    this.activityService.getByDestination(id).subscribe({
      next: (items) => this.activities.set(items),
      error: (err) => {
        this.snackBar.open(extractApiErrorMessage(err, 'Unable to refresh activities.'), 'Dismiss', {
          duration: 5000,
        });
      },
    });
  }
}
