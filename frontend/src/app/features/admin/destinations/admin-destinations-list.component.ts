import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { PagedResult } from '../../../core/models/common.models';
import { DestinationListItem } from '../../../core/models/destination.models';
import { DestinationService } from '../../../core/services/destination.service';
import { extractApiErrorMessage } from '../../../core/utils/api-error.util';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';

type StatusFilter = 'active' | 'inactive' | 'all';

@Component({
  selector: 'app-admin-destinations-list',
  imports: [
    CurrencyPipe,
    DecimalPipe,
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    LoadingSpinnerComponent,
  ],
  template: `
    <header class="page-header">
      <div>
        <h1>Destinations</h1>
        <p>Create, update, and deactivate destination listings.</p>
      </div>
      <a mat-flat-button color="primary" routerLink="/admin/destinations/new">
        <mat-icon>add</mat-icon>
        Add destination
      </a>
    </header>

    <form class="filters" [formGroup]="filters" (ngSubmit)="search()">
      <mat-form-field appearance="outline">
        <mat-label>Search</mat-label>
        <input matInput formControlName="search" placeholder="Name, city, country..." />
        <mat-icon matSuffix>search</mat-icon>
      </mat-form-field>
      <mat-form-field appearance="outline">
        <mat-label>Status</mat-label>
        <mat-select formControlName="status">
          <mat-option value="active">Active</mat-option>
          <mat-option value="inactive">Inactive</mat-option>
          <mat-option value="all">All</mat-option>
        </mat-select>
      </mat-form-field>
      <button mat-stroked-button type="submit">Search</button>
    </form>

    @if (loading()) {
      <app-loading-spinner message="Loading destinations..." />
    } @else if (error()) {
      <mat-card appearance="outlined" class="message-card">
        <mat-card-content>
          <p class="error">{{ error() }}</p>
          <button mat-flat-button type="button" (click)="reload()">Try again</button>
        </mat-card-content>
      </mat-card>
    } @else if (!destinations().length) {
      <mat-card appearance="outlined" class="message-card">
        <mat-card-content>
          <mat-icon class="empty-icon">travel_explore</mat-icon>
          <h2>No destinations found</h2>
          <p class="muted">
            Try a different search or status filter, or create your first destination.
          </p>
          <a mat-flat-button color="primary" routerLink="/admin/destinations/new">
            <mat-icon>add</mat-icon>
            Add destination
          </a>
        </mat-card-content>
      </mat-card>
    } @else {
      <div class="table-scroll">
        <table mat-table [dataSource]="destinations()" class="destinations-table">
          <ng-container matColumnDef="image">
            <th mat-header-cell *matHeaderCellDef>Image</th>
            <td mat-cell *matCellDef="let row">
              <img
                class="thumb"
                [src]="row.imageUrl || placeholder"
                [alt]="row.name"
                loading="lazy"
                (error)="onImageError($event)"
              />
            </td>
          </ng-container>

          <ng-container matColumnDef="name">
            <th mat-header-cell *matHeaderCellDef>Name</th>
            <td mat-cell *matCellDef="let row">
              <span class="strong">{{ row.name }}</span>
            </td>
          </ng-container>

          <ng-container matColumnDef="city">
            <th mat-header-cell *matHeaderCellDef>City</th>
            <td mat-cell *matCellDef="let row">{{ row.city }}</td>
          </ng-container>

          <ng-container matColumnDef="country">
            <th mat-header-cell *matHeaderCellDef>Country</th>
            <td mat-cell *matCellDef="let row">{{ row.country }}</td>
          </ng-container>

          <ng-container matColumnDef="climate">
            <th mat-header-cell *matHeaderCellDef>Climate</th>
            <td mat-cell *matCellDef="let row">{{ row.climate || '—' }}</td>
          </ng-container>

          <ng-container matColumnDef="averageDailyCost">
            <th mat-header-cell *matHeaderCellDef>Daily cost</th>
            <td mat-cell *matCellDef="let row">
              {{ row.averageDailyCost | currency: 'EUR' : 'symbol' : '1.0-0' }}
            </td>
          </ng-container>

          <ng-container matColumnDef="popularity">
            <th mat-header-cell *matHeaderCellDef>Popularity</th>
            <td mat-cell *matCellDef="let row">{{ row.popularity | number: '1.2-2' }}</td>
          </ng-container>

          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let row">
              <span class="badge" [class.inactive]="!row.isActive">
                {{ row.isActive ? 'Active' : 'Inactive' }}
              </span>
            </td>
          </ng-container>

          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef class="actions-header">Actions</th>
            <td mat-cell *matCellDef="let row" class="actions-cell">
              <a
                mat-icon-button
                matTooltip="Edit destination"
                [routerLink]="['/admin/destinations', row.id, 'edit']"
                aria-label="Edit destination"
              >
                <mat-icon>edit</mat-icon>
              </a>
              <button
                mat-icon-button
                type="button"
                [disabled]="!row.isActive || deactivatingId() === row.id"
                [matTooltip]="row.isActive ? 'Deactivate destination' : 'Already inactive'"
                aria-label="Deactivate destination"
                (click)="confirmDeactivate(row)"
              >
                <mat-icon>visibility_off</mat-icon>
              </button>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="columns"></tr>
          <tr mat-row *matRowDef="let row; columns: columns"></tr>
        </table>
      </div>

      <mat-paginator
        [length]="totalCount()"
        [pageIndex]="page() - 1"
        [pageSize]="pageSize()"
        [pageSizeOptions]="[10, 25, 50]"
        (page)="onPage($event)"
        aria-label="Destination pages"
      />
    }
  `,
  styles: `
    :host {
      display: block;
      width: min(1200px, 100%);
      margin: 0 auto;
      padding: 1.5rem 1rem 2rem;
      box-sizing: border-box;
    }

    .page-header {
      display: flex;
      flex-wrap: wrap;
      gap: 1rem;
      align-items: flex-start;
      justify-content: space-between;
      margin-bottom: 1.25rem;
    }

    .page-header h1 {
      font-family: Fraunces, Georgia, serif;
      margin: 0 0 0.25rem;
      color: #0d4f52;
    }

    .page-header p {
      margin: 0;
      color: var(--mat-sys-on-surface-variant);
    }

    .filters {
      display: flex;
      flex-wrap: wrap;
      gap: 0.75rem 1rem;
      align-items: center;
      margin-bottom: 1rem;
    }

    .filters mat-form-field {
      min-width: 200px;
      flex: 1 1 200px;
      max-width: 320px;
    }

    .table-scroll {
      overflow-x: auto;
      border: 1px solid var(--mat-sys-outline-variant);
      border-radius: 12px;
      background: var(--mat-sys-surface);
    }

    .destinations-table {
      width: 100%;
      min-width: 960px;
    }

    .thumb {
      width: 56px;
      height: 40px;
      object-fit: cover;
      border-radius: 6px;
      background: #e0f7fa;
      display: block;
    }

    .strong {
      font-weight: 600;
    }

    .badge {
      display: inline-block;
      padding: 0.15rem 0.6rem;
      border-radius: 999px;
      font-size: 0.75rem;
      font-weight: 600;
      background: rgba(13, 79, 82, 0.12);
      color: #0d4f52;
      white-space: nowrap;
    }

    .badge.inactive {
      background: var(--mat-sys-surface-container-high);
      color: var(--mat-sys-on-surface-variant);
    }

    .actions-header,
    .actions-cell {
      text-align: right;
      white-space: nowrap;
    }

    .message-card {
      max-width: 520px;
    }

    .message-card mat-card-content {
      display: flex;
      flex-direction: column;
      align-items: flex-start;
      gap: 0.5rem;
      padding-top: 1rem;
    }

    .message-card h2 {
      margin: 0;
      font-size: 1.1rem;
    }

    .empty-icon {
      font-size: 2.5rem;
      width: 2.5rem;
      height: 2.5rem;
      color: #0d4f52;
    }

    .muted {
      margin: 0;
      color: var(--mat-sys-on-surface-variant);
    }

    .error {
      margin: 0;
      color: var(--mat-sys-error);
    }
  `,
})
export class AdminDestinationsListComponent {
  private readonly fb = inject(FormBuilder);
  private readonly destinationService = inject(DestinationService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);

  readonly placeholder = '/assets/images/destination-placeholder.svg';
  readonly columns = [
    'image',
    'name',
    'city',
    'country',
    'climate',
    'averageDailyCost',
    'popularity',
    'status',
    'actions',
  ];

  readonly destinations = signal<DestinationListItem[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly page = signal(1);
  readonly pageSize = signal(10);
  readonly totalCount = signal(0);
  readonly deactivatingId = signal<number | null>(null);

  readonly filters = this.fb.nonNullable.group({
    search: '',
    status: 'active' as StatusFilter,
  });

  constructor() {
    this.filters.controls.search.valueChanges
      .pipe(debounceTime(400), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe(() => this.search());

    this.filters.controls.status.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed())
      .subscribe(() => this.search());

    this.reload();
  }

  search(): void {
    this.page.set(1);
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.error.set(null);

    const { search, status } = this.filters.getRawValue();

    this.destinationService
      .getDestinations({
        search: search.trim() || undefined,
        isActive: status === 'all' ? null : status === 'active',
        includeInactive: status === 'all',
        page: this.page(),
        pageSize: this.pageSize(),
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result: PagedResult<DestinationListItem>) => {
          if (!result.items.length && this.page() > 1) {
            this.page.set(1);
            this.reload();
            return;
          }

          this.destinations.set(result.items);
          this.totalCount.set(result.totalCount);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(extractApiErrorMessage(err, 'Unable to load destinations.'));
          this.loading.set(false);
        },
      });
  }

  onPage(event: PageEvent): void {
    this.page.set(event.pageIndex + 1);
    this.pageSize.set(event.pageSize);
    this.reload();
  }

  confirmDeactivate(destination: DestinationListItem): void {
    const data: ConfirmDialogData = {
      title: 'Deactivate destination',
      message:
        'The destination will no longer appear in public searches or new recommendations, but historical data will be preserved.',
      confirmText: 'Deactivate',
      cancelText: 'Cancel',
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
        if (confirmed) {
          this.deactivate(destination);
        }
      });
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = this.placeholder;
  }

  private deactivate(destination: DestinationListItem): void {
    this.deactivatingId.set(destination.id);

    this.destinationService.delete(destination.id).subscribe({
      next: () => {
        this.deactivatingId.set(null);
        this.snackBar.open(`${destination.name} deactivated`, 'Dismiss', { duration: 3000 });
        this.reload();
      },
      error: (err) => {
        this.deactivatingId.set(null);
        this.snackBar.open(
          extractApiErrorMessage(err, 'Unable to deactivate this destination.'),
          'Dismiss',
          { duration: 5000 }
        );
      },
    });
  }
}
