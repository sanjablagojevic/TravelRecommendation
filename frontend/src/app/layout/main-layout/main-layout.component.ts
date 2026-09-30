import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FooterComponent } from '../footer/footer.component';
import { NavbarComponent } from '../navbar/navbar.component';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, NavbarComponent, FooterComponent],
  template: `
    <div class="layout">
      <app-navbar />
      <main class="content">
        <router-outlet />
      </main>
      <app-footer />
    </div>
  `,
  styles: `
    .layout {
      min-height: 100dvh;
      display: flex;
      flex-direction: column;
      background:
        radial-gradient(ellipse 80% 50% at 10% -10%, rgba(0, 128, 128, 0.12), transparent),
        radial-gradient(ellipse 60% 40% at 100% 0%, rgba(194, 178, 128, 0.18), transparent),
        var(--mat-sys-surface);
    }

    .content {
      flex: 1;
      width: 100%;
      box-sizing: border-box;
    }
  `,
})
export class MainLayoutComponent {}
