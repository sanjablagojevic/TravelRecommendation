import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router, RouterLink } from '@angular/router';
import { AiService } from '../../core/services/ai.service';
import { TripPlanningStateService } from '../../core/services/trip-planning-state.service';
import { extractApiErrorMessage } from '../../core/utils/api-error.util';

const EXAMPLE_PROMPTS = [
  '7 days by the sea, €1200 budget, food and history.',
  'Relaxing 10-day trip with beaches and nature. Budget 2000 EUR.',
  'City break with museums, history and local food. 5 days, 1500 KM.',
] as const;

@Component({
  selector: 'app-ai-input',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <header class="page-header">
      <a mat-button routerLink="/plan-trip" class="back">
        <mat-icon>arrow_back</mat-icon>
        Back
      </a>
      <h1>Describe your ideal trip</h1>
      <p>
        Write in <strong>English</strong> or <strong>Bosnian (BHS)</strong>. Include budget, timing,
        trip style, and interests when you can.
      </p>
    </header>

    <form [formGroup]="form" (ngSubmit)="analyze()" class="form">
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Your trip description</mat-label>
        <textarea
          matInput
          formControlName="message"
          rows="8"
          placeholder="Example: A week in Croatia in June, beach and culture, about 1200 EUR..."
        ></textarea>
        @if (form.controls.message.touched && form.controls.message.hasError('required')) {
          <mat-error>Please describe your trip.</mat-error>
        }
        @if (form.controls.message.touched && form.controls.message.hasError('minlength')) {
          <mat-error>Use at least 10 characters so we can understand your request.</mat-error>
        }
      </mat-form-field>

      <section class="examples" aria-label="Example prompts">
        <p class="examples-label">Try an example:</p>
        <div class="example-chips">
          @for (example of examples; track example; let i = $index) {
            <button type="button" mat-stroked-button (click)="useExample(i)">
              Example {{ i + 1 }}
            </button>
          }
        </div>
      </section>

      @if (serviceUnavailable()) {
        <p class="service-msg">
          AI preference extraction is temporarily unavailable. You can still plan your trip with the
          <a routerLink="/plan-trip/manual">manual questionnaire</a>.
        </p>
      } @else if (errorMessage()) {
        <p class="error">{{ errorMessage() }}</p>
      }

      <div class="actions">
        <button
          mat-flat-button
          color="primary"
          type="submit"
          [disabled]="form.invalid || analyzing()"
        >
          @if (analyzing()) {
            <mat-spinner diameter="20" />
            Analyzing your request...
          } @else {
            Analyze preferences
          }
        </button>
      </div>
    </form>
  `,
  styles: `
    :host {
      display: block;
      width: min(720px, 100%);
      margin: 0 auto;
      padding: 1.5rem 1rem 2.5rem;
      box-sizing: border-box;
    }

    .back {
      margin: 0 0 0.5rem -0.5rem;
    }

    h1 {
      font-family: 'Fraunces', Georgia, serif;
      margin: 0 0 0.5rem;
      color: #0d4f52;
    }

    .page-header > p {
      margin: 0 0 1.5rem;
      color: var(--mat-sys-on-surface-variant);
      line-height: 1.55;
    }

    .full-width {
      width: 100%;
    }

    .examples-label {
      margin: 0 0 0.5rem;
      font-size: 0.875rem;
      color: var(--mat-sys-on-surface-variant);
    }

    .example-chips {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
      margin-bottom: 1.25rem;
    }

    .actions button {
      min-width: 12rem;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
    }

    .error {
      color: var(--mat-sys-error);
      margin: 0 0 1rem;
    }

    .service-msg {
      padding: 0.75rem 1rem;
      background: rgba(13, 115, 119, 0.08);
      border-radius: 8px;
      margin: 0 0 1rem;
      line-height: 1.5;
    }

    .service-msg a {
      color: #0d7377;
      font-weight: 600;
    }
  `,
})
export class AiInputComponent {
  private readonly fb = inject(FormBuilder);
  private readonly aiService = inject(AiService);
  private readonly state = inject(TripPlanningStateService);
  private readonly router = inject(Router);

  readonly examples = EXAMPLE_PROMPTS;
  readonly analyzing = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly serviceUnavailable = signal(false);

  readonly form = this.fb.nonNullable.group({
    message: ['', [Validators.required, Validators.minLength(10)]],
  });

  useExample(index: number): void {
    this.form.controls.message.setValue(this.examples[index]);
    this.form.controls.message.markAsDirty();
  }

  analyze(): void {
    if (this.form.invalid || this.analyzing()) {
      this.form.markAllAsTouched();
      return;
    }

    const message = this.form.controls.message.value.trim();
    this.analyzing.set(true);
    this.errorMessage.set(null);
    this.serviceUnavailable.set(false);

    this.aiService.extractPreferences(message).subscribe({
      next: (extraction) => {
        this.state.setExtraction(message, extraction);
        this.analyzing.set(false);
        void this.router.navigate(['/plan-trip/confirm']);
      },
      error: (err: unknown) => {
        this.analyzing.set(false);
        if (err instanceof HttpErrorResponse && err.status === 503) {
          this.serviceUnavailable.set(true);
          this.errorMessage.set(null);
          return;
        }
        this.errorMessage.set(extractApiErrorMessage(err, 'Could not analyze your request.'));
      },
    });
  }
}
