import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { extractApiErrorMessage } from '../../../core/utils/api-error.util';

@Component({
  selector: 'app-login',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSnackBarModule,
  ],
  template: `
    <div class="auth-shell">
      <mat-card>
        <mat-card-header>
          <mat-card-title>Sign in</mat-card-title>
          <mat-card-subtitle>Access favorites and your profile</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <form [formGroup]="form" (ngSubmit)="submit()">
            <mat-form-field appearance="outline" class="full">
              <mat-label>Email</mat-label>
              <input matInput type="email" formControlName="email" autocomplete="email" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="full">
              <mat-label>Password</mat-label>
              <input matInput type="password" formControlName="password" autocomplete="current-password" />
            </mat-form-field>
            <button mat-flat-button color="primary" type="submit" [disabled]="form.invalid || submitting()">
              Sign in
            </button>
          </form>
        </mat-card-content>
        <mat-card-actions align="end">
          <a mat-button routerLink="/register">Create an account</a>
        </mat-card-actions>
      </mat-card>
    </div>
  `,
  styles: `
    .auth-shell {
      max-width: 420px;
      margin: 2rem auto;
    }

    .full {
      width: 100%;
    }

    form {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }
  `,
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly snackBar = inject(MatSnackBar);

  readonly submitting = signal(false);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.submitting.set(false);
        this.snackBar.open('Signed in successfully.', 'Dismiss', { duration: 4000 });
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/';
        void this.router.navigateByUrl(returnUrl);
      },
      error: (err) => {
        this.submitting.set(false);
        this.snackBar.open(extractApiErrorMessage(err, 'Sign in failed.'), 'Dismiss', {
          duration: 5000,
        });
      },
    });
  }
}
