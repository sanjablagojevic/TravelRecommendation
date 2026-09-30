import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Category } from '../../../core/models/category.models';
import { CategoryService } from '../../../core/services/category.service';
import { extractApiErrorMessage } from '../../../core/utils/api-error.util';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { CategoryDialogComponent, CategoryDialogData } from './category-dialog.component';

@Component({
  selector: 'app-admin-categories',
  imports: [
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    LoadingSpinnerComponent,
  ],
  template: `
    <header class="page-header">
      <div>
        <h1>Categories</h1>
        <p>Organize destinations by travel category.</p>
      </div>
      <button mat-flat-button color="primary" type="button" (click)="openDialog()">
        <mat-icon>add</mat-icon>
        Add category
      </button>
    </header>

    @if (loading()) {
      <app-loading-spinner message="Loading categories..." />
    } @else if (error()) {
      <mat-card appearance="outlined" class="message-card">
        <mat-card-content>
          <p class="error">{{ error() }}</p>
          <button mat-flat-button type="button" (click)="load()">Try again</button>
        </mat-card-content>
      </mat-card>
    } @else if (!categories().length) {
      <mat-card appearance="outlined" class="message-card">
        <mat-card-content>
          <mat-icon class="empty-icon">category</mat-icon>
          <h2>No categories yet</h2>
          <p class="muted">Create your first category to group destinations.</p>
          <button mat-flat-button color="primary" type="button" (click)="openDialog()">
            <mat-icon>add</mat-icon>
            Add category
          </button>
        </mat-card-content>
      </mat-card>
    } @else {
      <div class="table-scroll">
        <table mat-table [dataSource]="categories()">
          <ng-container matColumnDef="name">
            <th mat-header-cell *matHeaderCellDef>Name</th>
            <td mat-cell *matCellDef="let row">
              <span class="strong">{{ row.name }}</span>
            </td>
          </ng-container>
          <ng-container matColumnDef="type">
            <th mat-header-cell *matHeaderCellDef>Type</th>
            <td mat-cell *matCellDef="let row">{{ row.type }}</td>
          </ng-container>
          <ng-container matColumnDef="description">
            <th mat-header-cell *matHeaderCellDef>Description</th>
            <td mat-cell *matCellDef="let row">{{ row.description || '—' }}</td>
          </ng-container>
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef class="actions-header">Actions</th>
            <td mat-cell *matCellDef="let row" class="actions-cell">
              <button
                mat-icon-button
                type="button"
                matTooltip="Edit category"
                aria-label="Edit category"
                (click)="openDialog(row)"
              >
                <mat-icon>edit</mat-icon>
              </button>
              <button
                mat-icon-button
                type="button"
                matTooltip="Delete category"
                aria-label="Delete category"
                [disabled]="deletingId() === row.id"
                (click)="confirmDelete(row)"
              >
                <mat-icon>delete</mat-icon>
              </button>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="columns"></tr>
          <tr mat-row *matRowDef="let row; columns: columns"></tr>
        </table>
      </div>
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

    .table-scroll {
      overflow-x: auto;
      border: 1px solid var(--mat-sys-outline-variant);
      border-radius: 12px;
      background: var(--mat-sys-surface);
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
export class AdminCategoriesComponent {
  private readonly categoryService = inject(CategoryService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);

  readonly columns = ['name', 'type', 'description', 'actions'];
  readonly categories = signal<Category[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly deletingId = signal<number | null>(null);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.categoryService
      .getAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (categories) => {
          this.categories.set(categories);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(extractApiErrorMessage(err, 'Unable to load categories.'));
          this.loading.set(false);
        },
      });
  }

  openDialog(category?: Category): void {
    const data: CategoryDialogData = { category };
    this.dialog
      .open<CategoryDialogComponent, CategoryDialogData, Category>(CategoryDialogComponent, {
        data,
        width: '520px',
      })
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((saved) => {
        if (saved) {
          this.load();
        }
      });
  }

  confirmDelete(category: Category): void {
    const data: ConfirmDialogData = {
      title: 'Delete category',
      message: `"${category.name}" will be removed. Categories that are still assigned to destinations cannot be deleted.`,
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
        if (confirmed) {
          this.delete(category);
        }
      });
  }

  private delete(category: Category): void {
    this.deletingId.set(category.id);

    this.categoryService.delete(category.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.snackBar.open('Category deleted', 'Dismiss', { duration: 3000 });
        this.load();
      },
      error: (err) => {
        this.deletingId.set(null);
        this.snackBar.open(
          extractApiErrorMessage(err, 'Unable to delete this category.'),
          'Dismiss',
          { duration: 5000 }
        );
      },
    });
  }
}
