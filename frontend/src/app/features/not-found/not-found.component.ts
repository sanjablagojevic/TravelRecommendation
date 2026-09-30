import { Component } from '@angular/core';

import { MatButtonModule } from '@angular/material/button';

import { RouterLink } from '@angular/router';



@Component({

  selector: 'app-not-found',

  imports: [RouterLink, MatButtonModule],

  template: `

    <section class="not-found">

      <h1>Page not found.</h1>

      <p>The page you are looking for does not exist or may have moved.</p>

      <a mat-flat-button color="primary" routerLink="/">Back to home</a>

    </section>

  `,

  styles: `

    .not-found {

      text-align: center;

      padding: 4rem 1rem;

      max-width: 480px;

      margin: 0 auto;

    }



    h1 {

      font-family: 'Fraunces', Georgia, serif;

      font-size: 2rem;

      margin: 0 0 0.75rem;

      color: #0d4f52;

    }



    p {

      margin: 0 0 1.5rem;

      color: var(--mat-sys-on-surface-variant);

      line-height: 1.5;

    }

  `,

})

export class NotFoundComponent {}


