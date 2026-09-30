import { Component } from '@angular/core';

@Component({
  selector: 'app-footer',
  template: `
    <footer class="site-footer">
      <p>
        <strong>Travel Recommendation</strong> · {{ year }} · Intelligent Travel Recommendation System
      </p>
    </footer>
  `,
  styles: `
    .site-footer {
      margin-top: auto;
      padding: 1.25rem 1rem;
      text-align: center;
      border-top: 1px solid var(--mat-sys-outline-variant);
      background: var(--mat-sys-surface-container-low);
      color: var(--mat-sys-on-surface-variant);
      font-size: 0.875rem;
    }

    p {
      margin: 0;
    }

    strong {
      color: var(--mat-sys-on-surface);
      font-family: 'Fraunces', Georgia, serif;
    }
  `,
})
export class FooterComponent {
  readonly year = new Date().getFullYear();
}
