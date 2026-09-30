import { Component, inject, signal } from '@angular/core';

import { MatButtonModule } from '@angular/material/button';

import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { RouterLink } from '@angular/router';

import { DestinationListItem } from '../../core/models/destination.models';

import { AuthService } from '../../core/services/auth.service';

import { DestinationService } from '../../core/services/destination.service';

import { DestinationCardComponent } from '../../shared/components/destination-card/destination-card.component';



@Component({

  selector: 'app-home',

  imports: [

    RouterLink,

    MatButtonModule,

    MatProgressSpinnerModule,

    DestinationCardComponent,

  ],

  template: `

    <section class="hero">

      <div class="hero-inner">

        <p class="eyebrow">Intelligent Travel Recommendation System</p>

        <h1>Discover your next destination</h1>

        <p class="subtitle">

          @if (auth.isAuthenticated()) {

            Welcome back, {{ auth.getCurrentUser()?.firstName }} — explore trips tailored to your

            interests and favorites.

          } @else {

            Personalized recommendations based on your interests, budget, and travel style — start

            exploring in seconds.

          }

        </p>

        <div class="hero-actions">

          <a mat-flat-button color="primary" routerLink="/destinations">Explore destinations</a>

          @if (auth.isAuthenticated()) {

            <a mat-button class="plan-link" routerLink="/plan-trip">Plan a trip</a>

          } @else {

            <a mat-stroked-button routerLink="/register">Create account</a>

          }

        </div>

      </div>

      <div class="hero-wave" aria-hidden="true"></div>

    </section>



    <section class="featured">

      <div class="featured-header">

        <h2>Featured destinations</h2>

        <a mat-button color="primary" routerLink="/destinations">View all</a>

      </div>



      @if (loading()) {

        <div class="loading">

          <mat-spinner diameter="40" />

        </div>

      } @else if (destinations().length) {

        <div class="grid">

          @for (destination of destinations(); track destination.id) {

            <app-destination-card [destination]="destination" />

          }

        </div>

      } @else {

        <p class="empty">No destinations available yet. Check back soon.</p>

      }

    </section>

  `,

  styles: `

    .hero {

      position: relative;

      margin: 0 0 2rem;

      padding: clamp(3rem, 8vw, 5.5rem) 1.25rem 4.5rem;

      background: linear-gradient(145deg, #0d7377 0%, #14919b 45%, #2a9d8f 100%);

      color: #f8fffe;

      overflow: hidden;

    }



    .hero-inner {

      max-width: 720px;

      margin: 0 auto;

      text-align: center;

    }



    .eyebrow {

      margin: 0 0 0.75rem;

      font-size: 0.875rem;

      letter-spacing: 0.08em;

      text-transform: uppercase;

      opacity: 0.9;

    }



    h1 {

      font-family: 'Fraunces', Georgia, serif;

      font-size: clamp(2rem, 5vw, 3rem);

      font-weight: 600;

      margin: 0 0 1rem;

      line-height: 1.15;

    }



    .subtitle {

      margin: 0 auto 1.75rem;

      max-width: 36rem;

      font-size: 1.0625rem;

      line-height: 1.6;

      opacity: 0.95;

    }



    .hero-actions {

      display: flex;

      flex-wrap: wrap;

      gap: 0.75rem;

      justify-content: center;

    }



    .plan-link {

      color: #f8fffe;

      opacity: 0.95;

    }



    .hero-wave {

      position: absolute;

      bottom: -1px;

      left: 0;

      right: 0;

      height: 48px;

      background: var(--mat-sys-surface);

      clip-path: ellipse(75% 100% at 50% 100%);

    }



    .featured-header {

      display: flex;

      align-items: baseline;

      justify-content: space-between;

      gap: 1rem;

      margin-bottom: 1.25rem;

    }



    h2 {

      font-family: 'Fraunces', Georgia, serif;

      font-size: 1.75rem;

      margin: 0;

      color: #0d4f52;

    }



    .grid {

      display: grid;

      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));

      gap: 1.25rem;

    }



    .loading {

      display: flex;

      justify-content: center;

      padding: 2rem;

    }



    .empty {

      color: var(--mat-sys-on-surface-variant);

      text-align: center;

      padding: 2rem;

    }

    .featured {
      width: min(1200px, 100%);
      margin: 0 auto;
      padding: 0 1rem 2.5rem;
      box-sizing: border-box;
    }

  `,

})

export class HomeComponent {

  protected readonly auth = inject(AuthService);

  private readonly destinationService = inject(DestinationService);



  readonly loading = signal(true);

  readonly destinations = signal<DestinationListItem[]>([]);



  constructor() {

    this.destinationService.getDestinations({ page: 1, pageSize: 6 }).subscribe({

      next: (result) => {

        this.destinations.set(result.items);

        this.loading.set(false);

      },

      error: () => {

        this.destinations.set([]);

        this.loading.set(false);

      },

    });

  }

}


