import { Component, inject, signal } from '@angular/core';

import { MatButtonModule } from '@angular/material/button';

import { MatIconModule } from '@angular/material/icon';

import { MatListModule } from '@angular/material/list';

import { MatSidenavModule } from '@angular/material/sidenav';

import { MatToolbarModule } from '@angular/material/toolbar';

import { RouterLink, RouterLinkActive } from '@angular/router';

import { AuthService } from '../../core/services/auth.service';



@Component({

  selector: 'app-navbar',

  imports: [

    RouterLink,

    RouterLinkActive,

    MatToolbarModule,

    MatButtonModule,

    MatIconModule,

    MatSidenavModule,

    MatListModule,

  ],

  template: `

    <mat-toolbar color="primary" class="toolbar">

      <button

        mat-icon-button

        type="button"

        class="menu-btn"

        aria-label="Open navigation menu"

        (click)="mobileOpen.set(true)"

      >

        <mat-icon>menu</mat-icon>

      </button>



      <a routerLink="/" class="brand">

        <mat-icon>travel_explore</mat-icon>

        <span>Travel Recommendation</span>

      </a>



      <span class="spacer"></span>



      <nav class="nav-desktop">

        <a mat-button routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }">

          Home

        </a>

        <a mat-button routerLink="/destinations" routerLinkActive="active">Destinations</a>

        @if (auth.isAuthenticated()) {

          <a mat-button routerLink="/plan-trip" routerLinkActive="active">Plan a Trip</a>

          <a mat-button routerLink="/itineraries" routerLinkActive="active">My Trips</a>

          <a mat-button routerLink="/favorites" routerLinkActive="active">Favorites</a>

          <a mat-button routerLink="/recommendation-history" routerLinkActive="active">History</a>

        }

        @if (auth.isAuthenticated()) {

          <a mat-button routerLink="/profile" routerLinkActive="active">Profile</a>

        }

        @if (auth.isAdmin()) {

          <a mat-button routerLink="/admin" routerLinkActive="active">Admin</a>

        }

        @if (auth.isAuthenticated()) {

          <button mat-button type="button" (click)="auth.logout()">Logout</button>

        } @else {

          <a mat-button routerLink="/login" routerLinkActive="active">Login</a>

          <a mat-flat-button class="register-btn" routerLink="/register">Register</a>

        }

      </nav>

    </mat-toolbar>



    <mat-sidenav-container class="mobile-drawer-container">

      <mat-sidenav

        mode="over"

        position="start"

        [opened]="mobileOpen()"

        (closedStart)="mobileOpen.set(false)"

      >

        <mat-nav-list>

          <a mat-list-item routerLink="/" (click)="closeMobile()" routerLinkActive="active">Home</a>

          <a mat-list-item routerLink="/destinations" (click)="closeMobile()" routerLinkActive="active">

            Destinations

          </a>

          @if (auth.isAuthenticated()) {

            <a mat-list-item routerLink="/plan-trip" (click)="closeMobile()" routerLinkActive="active">

              Plan a Trip

            </a>

            <a mat-list-item routerLink="/itineraries" (click)="closeMobile()" routerLinkActive="active">

              My Trips

            </a>

            <a mat-list-item routerLink="/favorites" (click)="closeMobile()" routerLinkActive="active">

              Favorites

            </a>

            <a mat-list-item routerLink="/recommendation-history" (click)="closeMobile()" routerLinkActive="active">

              History

            </a>

          }

          @if (auth.isAuthenticated()) {

            <a mat-list-item routerLink="/profile" (click)="closeMobile()" routerLinkActive="active">

              Profile

            </a>

          }

          @if (auth.isAdmin()) {

            <a mat-list-item routerLink="/admin" (click)="closeMobile()" routerLinkActive="active">Admin</a>

          }

          @if (auth.isAuthenticated()) {

            <button mat-list-item type="button" (click)="logoutMobile()">Logout</button>

          } @else {

            <a mat-list-item routerLink="/login" (click)="closeMobile()" routerLinkActive="active">Login</a>

            <a mat-list-item routerLink="/register" (click)="closeMobile()" routerLinkActive="active">

              Register

            </a>

          }

        </mat-nav-list>

      </mat-sidenav>

    </mat-sidenav-container>

  `,

  styles: `

    .toolbar {

      position: sticky;

      top: 0;

      z-index: 100;

    }



    .brand {

      display: inline-flex;

      align-items: center;

      gap: 0.5rem;

      color: inherit;

      text-decoration: none;

      font-weight: 600;

      font-family: 'Fraunces', Georgia, serif;

    }



    .spacer {

      flex: 1;

    }



    .nav-desktop {

      display: none;

      align-items: center;

      gap: 0.15rem;

      flex-wrap: wrap;

    }



    .menu-btn {

      display: inline-flex;

    }



    .register-btn {

      margin-left: 0.25rem;

      background: rgba(255, 255, 255, 0.15);

      color: inherit;

    }



    a.active {

      font-weight: 600;

    }



    .mobile-drawer-container {

      position: fixed;

      inset: 0;

      pointer-events: none;

      z-index: 99;

    }



    mat-sidenav {

      width: 260px;

      pointer-events: auto;

    }



    @media (min-width: 840px) {

      .menu-btn {

        display: none;

      }



      .nav-desktop {

        display: flex;

      }



      .mobile-drawer-container {

        display: none;

      }

    }

  `,

})

export class NavbarComponent {

  protected readonly auth = inject(AuthService);

  readonly mobileOpen = signal(false);



  closeMobile(): void {

    this.mobileOpen.set(false);

  }



  logoutMobile(): void {

    this.closeMobile();

    this.auth.logout();

  }

}


