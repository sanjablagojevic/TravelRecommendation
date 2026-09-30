import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-plan-trip-landing',
  imports: [RouterLink, MatCardModule, MatButtonModule, MatIconModule],
  template: `
    <header class="page-header">
      <h1>How would you like to plan your trip?</h1>
      <p class="note">
        Our recommendation engine ranks destinations from your confirmed preferences — budget,
        interests, trip style, and more.
      </p>
    </header>

    <div class="options">
      <mat-card class="option-card">
        <mat-card-header>
          <mat-icon mat-card-avatar class="option-icon">checklist</mat-icon>
          <mat-card-title>Choose my preferences</mat-card-title>
          <mat-card-subtitle>Select your budget, trip length and interests.</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <p>
            Step through budget, duration, trip type, interests, travel month, and climate.
          </p>
        </mat-card-content>
        <mat-card-actions align="end">
          <a mat-flat-button color="primary" routerLink="/plan-trip/manual">Start questionnaire</a>
        </mat-card-actions>
      </mat-card>

      <mat-card class="option-card">
        <mat-card-header>
          <mat-icon mat-card-avatar class="option-icon ai">auto_awesome</mat-icon>
          <mat-card-title>Tell AI what I'm looking for</mat-card-title>
          <mat-card-subtitle>Describe your ideal trip in your own words.</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <p>
            We extract structured preferences for you to review before the recommendation engine
            ranks destinations.
          </p>
        </mat-card-content>
        <mat-card-actions align="end">
          <a mat-stroked-button color="primary" routerLink="/plan-trip/ai">Describe my trip</a>
        </mat-card-actions>
      </mat-card>
    </div>

    <p class="engine-note">
      Your final destinations are ranked by our recommendation engine based on your confirmed
      preferences.
    </p>
  `,
  styles: `
    :host {
      display: block;
      width: min(960px, 100%);
      margin: 0 auto;
      padding: 1.5rem 1rem 2.5rem;
      box-sizing: border-box;
    }

    .page-header h1 {
      font-family: 'Fraunces', Georgia, serif;
      font-size: clamp(1.75rem, 4vw, 2.25rem);
      margin: 0 0 0.75rem;
      color: #0d4f52;
    }

    .note {
      margin: 0 0 2rem;
      color: var(--mat-sys-on-surface-variant);
      line-height: 1.55;
      max-width: 42rem;
    }

    .options {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 1.25rem;
    }

    .option-card {
      display: flex;
      flex-direction: column;
      height: 100%;
    }

    .option-icon {
      background: rgba(13, 115, 119, 0.12);
      color: #0d7377;
      padding: 0.5rem;
      border-radius: 50%;
      width: 2.5rem;
      height: 2.5rem;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .option-icon.ai {
      background: rgba(42, 157, 143, 0.15);
      color: #2a9d8f;
    }

    mat-card-content p {
      margin: 0;
      color: var(--mat-sys-on-surface-variant);
      line-height: 1.5;
    }

    mat-card-actions {
      margin-top: auto;
      padding-top: 0.5rem;
    }

    .engine-note {
      margin: 1.75rem 0 0;
      color: var(--mat-sys-on-surface-variant);
      font-size: 0.9375rem;
      line-height: 1.5;
      max-width: 40rem;
    }
  `,
})
export class PlanTripLandingComponent {}
