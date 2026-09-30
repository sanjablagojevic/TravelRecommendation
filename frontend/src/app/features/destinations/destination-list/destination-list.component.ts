import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin, switchMap } from 'rxjs';
import { CategoryService } from '../../../core/services/category.service';
import { DestinationService } from '../../../core/services/destination.service';
import { InterestService } from '../../../core/services/interest.service';
import { DestinationListItem } from '../../../core/models/destination.models';
import { Category, Interest } from '../../../core/models/category.models';
import { PagedResult } from '../../../core/models/common.models';
import { DestinationCardComponent } from '../../../shared/components/destination-card/destination-card.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { extractApiErrorMessage } from '../../../core/utils/api-error.util';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-destination-list',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatPaginatorModule,
    DestinationCardComponent,
    LoadingSpinnerComponent,
  ],
  template: `
    <header class="page-header">
      <h1>Destinations</h1>
      <p>Search and filter travel destinations from the catalog.</p>
    </header>

    <form class="filters" [formGroup]="filters" (ngSubmit)="applyFilters()">
      <mat-form-field appearance="outline">
        <mat-label>Search</mat-label>
        <input matInput formControlName="search" placeholder="Name, city, country..." />
      </mat-form-field>
      <mat-form-field appearance="outline">
        <mat-label>Country</mat-label>
        <input matInput formControlName="country" />
      </mat-form-field>
      <mat-form-field appearance="outline">
        <mat-label>City</mat-label>
        <input matInput formControlName="city" />
      </mat-form-field>
      <mat-form-field appearance="outline">
        <mat-label>Category</mat-label>
        <mat-select formControlName="categoryId">
          <mat-option [value]="null">All categories</mat-option>
          @for (category of categories(); track category.id) {
            <mat-option [value]="category.id">{{ category.name }}</mat-option>
          }
        </mat-select>
      </mat-form-field>
      <mat-form-field appearance="outline">
        <mat-label>Interest</mat-label>
        <mat-select formControlName="interestId">
          <mat-option [value]="null">All interests</mat-option>
          @for (interest of interests(); track interest.id) {
            <mat-option [value]="interest.id">{{ interest.name }}</mat-option>
          }
        </mat-select>
      </mat-form-field>
      <button mat-flat-button color="primary" type="submit">Apply</button>
      <button mat-button type="button" (click)="resetFilters()">Reset</button>
    </form>

    @if (loading()) {
      <app-loading-spinner message="Loading destinations..." />
    } @else if (error()) {
      <p class="error">{{ error() }}</p>
    } @else if (!destinations().length) {
      <p class="empty">No destinations match your filters.</p>
    } @else {
      <div class="grid">
        @for (destination of destinations(); track destination.id) {
          <app-destination-card [destination]="destination" />
        }
      </div>
      <mat-paginator
        [length]="totalCount()"
        [pageIndex]="page() - 1"
        [pageSize]="pageSize()"
        [pageSizeOptions]="[6, 12, 24]"
        (page)="onPage($event)"
        aria-label="Destination pages"
      />
    }
  `,
  styles: `
    .page-header h1 {
      margin: 0 0 0.25rem;
    }

    .page-header p {
      margin: 0 0 1.5rem;
      color: var(--mat-sys-on-surface-variant);
    }

    .filters {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      gap: 0.75rem 1rem;
      align-items: start;
      margin-bottom: 1.5rem;
    }

    .grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 1rem;
      margin-bottom: 1rem;
    }

    .error {
      color: var(--mat-sys-error);
    }

    .empty {
      color: var(--mat-sys-on-surface-variant);
    }

    :host {
      display: block;
      width: min(1200px, 100%);
      margin: 0 auto;
      padding: 1.5rem 1rem 2rem;
      box-sizing: border-box;
    }
  `,
})
export class DestinationListComponent {
  private readonly fb = inject(FormBuilder);
  private readonly destinationService = inject(DestinationService);
  private readonly categoryService = inject(CategoryService);
  private readonly interestService = inject(InterestService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  readonly categories = signal<Category[]>([]);
  readonly interests = signal<Interest[]>([]);
  readonly destinations = signal<DestinationListItem[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly page = signal(1);
  readonly pageSize = signal(12);
  readonly totalCount = signal(0);

  readonly filters = this.fb.group({
    search: [''],
    country: [''],
    city: [''],
    categoryId: this.fb.control<number | null>(null),
    interestId: this.fb.control<number | null>(null),
  });

  constructor() {
    forkJoin({
      categories: this.categoryService.getAll(),
      interests: this.interestService.getAll(),
    })
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: ({ categories, interests }) => {
          this.categories.set(categories);
          this.interests.set(interests);
        },
        error: (err) => {
          this.snackBar.open(extractApiErrorMessage(err), 'Dismiss', { duration: 5000 });
        },
      });

    this.route.queryParamMap
      .pipe(
        switchMap((params) => {
          this.loading.set(true);
          this.error.set(null);
          this.filters.patchValue(
            {
              search: params.get('search') ?? '',
              country: params.get('country') ?? '',
              city: params.get('city') ?? '',
              categoryId: this.parseOptionalInt(params.get('categoryId')),
              interestId: this.parseOptionalInt(params.get('interestId')),
            },
            { emitEvent: false }
          );
          const page = this.parsePositiveInt(params.get('page'), 1);
          const pageSize = this.parsePositiveInt(params.get('pageSize'), 12);
          this.page.set(page);
          this.pageSize.set(pageSize);

          return this.destinationService.getDestinations({
            search: params.get('search') ?? undefined,
            country: params.get('country') ?? undefined,
            city: params.get('city') ?? undefined,
            categoryId: this.parseOptionalInt(params.get('categoryId')) ?? undefined,
            interestId: this.parseOptionalInt(params.get('interestId')) ?? undefined,
            page,
            pageSize,
          });
        }),
        takeUntilDestroyed()
      )
      .subscribe({
        next: (result: PagedResult<DestinationListItem>) => {
          this.destinations.set(result.items);
          this.totalCount.set(result.totalCount);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(extractApiErrorMessage(err));
          this.loading.set(false);
        },
      });

  }

  applyFilters(): void {
    const value = this.filters.getRawValue();
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        search: value.search?.trim() || null,
        country: value.country?.trim() || null,
        city: value.city?.trim() || null,
        categoryId: value.categoryId ?? null,
        interestId: value.interestId ?? null,
        page: 1,
        pageSize: this.pageSize(),
      },
    });
  }

  resetFilters(): void {
    this.filters.reset({
      search: '',
      country: '',
      city: '',
      categoryId: null,
      interestId: null,
    });
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page: 1, pageSize: 12 },
    });
  }

  onPage(event: PageEvent): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        page: event.pageIndex + 1,
        pageSize: event.pageSize,
      },
      queryParamsHandling: 'merge',
    });
  }

  private parseOptionalInt(value: string | null): number | null {
    if (!value) {
      return null;
    }
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : null;
  }

  private parsePositiveInt(value: string | null, fallback: number): number {
    const parsed = Number(value);
    return Number.isFinite(parsed) && parsed > 0 ? parsed : fallback;
  }
}
