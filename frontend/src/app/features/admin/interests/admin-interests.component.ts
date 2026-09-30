import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Interest } from '../../../core/models/category.models';
import { InterestService } from '../../../core/services/interest.service';
import { extractApiErrorMessage } from '../../../core/utils/api-error.util';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { InterestDialogComponent, InterestDialogData } from './interest-dialog.component';

@Component({
  selector: 'app-admin-interests',
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
        <h1>Interests</h1>
        <p>Define the interest tags used when matching travellers to destinations.</p>
      </div>
      <button mat-flat-button color="primary" type="button" (click)="openDialog()">
        <mat-icon>add</mat-icon>
        Add interest
      </button>
    </header>

    <p class="info">
      Interests are used by the recommendation engine to match user preferences with destinations.
    </p>

    @if (loading()) {
      <app-loading-spinner message="Loading interests..." />
    } @else if (error()) {
      <mat-card appearance="outlined" class="message-card">
        <mat-card-content>
          <p class="error">{{ error() }}</p>
          <button mat-flat-button type="button" (click)="load()">Try again</button>
        </mat-card-content>
      </mat-card>
    } @else if (!interests().length) {
      <mat-card appearance="outlined" class="message-card">
        <mat-card-content>
          <mat-icon class="empty-icon">interests</mat-icon>
          <h2>No interests yet</h2>
          <p class="muted">Create your first interest tag to power recommendations.</p>
          <button mat-flat-button color="primary" type="button" (click)="openDialog()">
            <mat-icon>add</mat-icon>
            Add interest
          </button>
        </mat-card-content>
      </mat-card>
    } @else {
      <div class="table-scroll">
        <table mat-table [dataSource]="interests()">
          <ng-container matColumnDef="name">
            <th mat-header-cell *matHeaderCellDef>Name</th>
            <td mat-cell *matCellDef="let row">
              <span class="strong">{{ row.name }}</span>
            </td>
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
                matTooltip="Edit interest"
                aria-label="Edit interest"
                (click)="openDialog(row)"
              >
                <mat-icon>edit</mat-icon>
              </button>
              <button
                mat-icon-button
                type="button"
                matTooltip="Delete interest"
                aria-label="Delete interest"
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
      margin-bottom: 0.75rem;
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

    .info {
      margin: 0 0 1.25rem;
      padding: 0.75rem 1rem;
      border-radius: 10px;
      background: rgba(13, 79, 82, 0.08);
      color: #0d4f52;
      font-size: 0.875rem;
    }

    .table-scroll {
      overflow-x: auto;
      border: 1px solid var(--mat-sys-outline-variant);
      border-radius: 12px;
      background: var(--mat-sys-surface);
    }

    table {
      width: 100%;
      min-width: 560px;
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
export class AdminInterestsComponent {
  private readonly interestService = inject(InterestService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);

  readonly columns = ['name', 'description', 'actions'];
  readonly interests = signal<Interest[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly deletingId = signal<number | null>(null);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.interestService
      .getAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (interests) => {
          this.interests.set(interests);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(extractApiErrorMessage(err, 'Unable to load interests.'));
          this.loading.set(false);
        },
      });
  }

  openDialog(interest?: Interest): void {
    const data: InterestDialogData = { interest };
    this.dialog
      .open<InterestDialogComponent, InterestDialogData, Interest>(InterestDialogComponent, {
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

  confirmDelete(interest: Interest): void {
    const data: ConfirmDialogData = {
      title: 'Delete interest',
      message: `"${interest.name}" will be removed. Interests that are still assigned to destinations cannot be deleted.`,
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
          this.delete(interest);
        }
      });
  }

  private delete(interest: Interest): void {
    this.deletingId.set(interest.id);

    this.interestService.delete(interest.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.snackBar.open('Interest deleted', 'Dismiss', { duration: 3000 });
        this.load();
      },
      error: (err) => {
        this.deletingId.set(null);
        this.snackBar.open(
          extractApiErrorMessage(err, 'Unable to delete this interest.'),
          'Dismiss',
          { duration: 5000 }
        );
      },
    });
  }
}
