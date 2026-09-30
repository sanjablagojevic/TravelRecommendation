import { BreakpointObserver } from '@angular/cdk/layout';
import { Component, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatSidenavModule } from '@angular/material/sidenav';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { map } from 'rxjs';

interface AdminNavLink {
  label: string;
  path: string;
  icon: string;
  exact: boolean;
}

const MOBILE_QUERY = '(max-width: 959px)';

@Component({
  selector: 'app-admin-layout',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatDividerModule,
  ],
  template: `
    <mat-sidenav-container class="admin-shell" [hasBackdrop]="isMobile()">
      <mat-sidenav
        class="admin-nav"
        [mode]="isMobile() ? 'over' : 'side'"
        [opened]="opened()"
        (closedStart)="opened.set(false)"
      >
        <div class="nav-header">
          <mat-icon>admin_panel_settings</mat-icon>
          <span>Admin</span>
        </div>
        <mat-divider />
        <mat-nav-list>
          @for (link of links; track link.path) {
            <a
              mat-list-item
              [routerLink]="link.path"
              routerLinkActive="active"
              [routerLinkActiveOptions]="{ exact: link.exact }"
              (click)="onNavigate()"
            >
              <mat-icon matListItemIcon>{{ link.icon }}</mat-icon>
              <span matListItemTitle>{{ link.label }}</span>
            </a>
          }
        </mat-nav-list>
      </mat-sidenav>

      <mat-sidenav-content class="admin-content">
        @if (isMobile()) {
          <div class="mobile-bar">
            <button
              mat-icon-button
              type="button"
              aria-label="Toggle admin navigation"
              (click)="opened.set(!opened())"
            >
              <mat-icon>menu</mat-icon>
            </button>
            <span class="mobile-title">Admin</span>
          </div>
        }
        <router-outlet />
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: `
    .admin-shell {
      min-height: calc(100dvh - 64px);
      background: transparent;
    }

    .admin-nav {
      width: 240px;
      border-right: 1px solid var(--mat-sys-outline-variant);
      background: var(--mat-sys-surface-container-low);
    }

    .nav-header {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 1rem 1.25rem;
      font-family: Fraunces, Georgia, serif;
      font-size: 1.1rem;
      font-weight: 600;
      color: #0d4f52;
    }

    .admin-content {
      background: transparent;
    }

    .mobile-bar {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.5rem 0.75rem 0;
    }

    .mobile-title {
      font-family: Fraunces, Georgia, serif;
      font-weight: 600;
      color: #0d4f52;
    }

    a.active {
      background: rgba(13, 79, 82, 0.1);
      font-weight: 600;
    }
  `,
})
export class AdminLayoutComponent {
  private readonly breakpoints = inject(BreakpointObserver);

  readonly links: AdminNavLink[] = [
    { label: 'Dashboard', path: '/admin', icon: 'dashboard', exact: true },
    { label: 'Destinations', path: '/admin/destinations', icon: 'public', exact: false },
    { label: 'Categories', path: '/admin/categories', icon: 'category', exact: false },
    { label: 'Interests', path: '/admin/interests', icon: 'interests', exact: false },
  ];

  readonly isMobile = toSignal(
    this.breakpoints.observe(MOBILE_QUERY).pipe(map((state) => state.matches)),
    { initialValue: this.breakpoints.isMatched(MOBILE_QUERY) }
  );

  readonly opened = signal(!this.breakpoints.isMatched(MOBILE_QUERY));

  constructor() {
    effect(() => {
      this.opened.set(!this.isMobile());
    });
  }

  onNavigate(): void {
    if (this.isMobile()) {
      this.opened.set(false);
    }
  }
}
