import { Component, input } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-loading-spinner',
  imports: [MatProgressSpinnerModule],
  template: `
    <div class="spinner-wrap" [class.inline]="inline()">
      <mat-spinner [diameter]="diameter()"></mat-spinner>
      @if (message()) {
        <p>{{ message() }}</p>
      }
    </div>
  `,
  styles: `
    .spinner-wrap {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 1rem;
      padding: 2rem;
      color: var(--mat-sys-on-surface-variant);
    }

    .spinner-wrap.inline {
      padding: 1rem;
    }

    p {
      margin: 0;
    }
  `,
})
export class LoadingSpinnerComponent {
  readonly message = input<string | null>(null);
  readonly diameter = input(48);
  readonly inline = input(false);
}
