import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  confirmColor?: 'warn' | 'primary';
}

@Component({
  selector: 'app-confirm-dialog',
  imports: [MatDialogModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>{{ data.title }}</h2>
    <mat-dialog-content>
      <p>{{ data.message }}</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" (click)="cancel()">
        {{ data.cancelText ?? 'Cancel' }}
      </button>
      <button
        mat-flat-button
        type="button"
        [class.danger]="(data.confirmColor ?? 'warn') === 'warn'"
        (click)="confirm()"
      >
        {{ data.confirmText ?? 'Confirm' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: `
    h2 {
      font-family: Fraunces, Georgia, serif;
      color: #0d4f52;
    }

    p {
      margin: 0;
      color: var(--mat-sys-on-surface-variant);
    }

    .danger {
      --mat-button-filled-container-color: var(--mat-sys-error);
      --mat-button-filled-label-text-color: var(--mat-sys-on-error);
      --mdc-filled-button-container-color: var(--mat-sys-error);
      --mdc-filled-button-label-text-color: var(--mat-sys-on-error);
    }
  `,
})
export class ConfirmDialogComponent {
  private readonly dialogRef = inject<MatDialogRef<ConfirmDialogComponent, boolean>>(MatDialogRef);
  readonly data = inject<ConfirmDialogData>(MAT_DIALOG_DATA);

  confirm(): void {
    this.dialogRef.close(true);
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
