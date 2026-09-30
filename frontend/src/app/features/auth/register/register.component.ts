import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import {
  passwordMatchValidator,
  passwordStrengthValidator,
} from '../../../core/validators/password.validators';
import { extractApiErrorMessage } from '../../../core/utils/api-error.util';

@Component({
  selector: 'app-register',
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
          <mat-card-title>Create account</mat-card-title>
          <mat-card-subtitle>Join the travel portal</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <form [formGroup]="form" (ngSubmit)="submit()">
            <mat-form-field appearance="outline" class="full">
              <mat-label>First name</mat-label>
              <input matInput formControlName="firstName" autocomplete="given-name" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="full">
              <mat-label>Last name</mat-label>
              <input matInput formControlName="lastName" autocomplete="family-name" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="full">
              <mat-label>Email</mat-label>
              <input matInput type="email" formControlName="email" autocomplete="email" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="full">
              <mat-label>Password</mat-label>
              <input matInput type="password" formControlName="password" autocomplete="new-password" />
              @if (form.controls.password.hasError('passwordStrength')) {
                <mat-error>{{ form.controls.password.getError('passwordStrength') }}</mat-error>
              }
            </mat-form-field>
            <mat-form-field appearance="outline" class="full">
              <mat-label>Confirm password</mat-label>
              <input matInput type="password" formControlName="confirmPassword" autocomplete="new-password" />
              @if (form.hasError('passwordMismatch')) {
                <mat-error>Passwords do not match</mat-error>
              }
            </mat-form-field>
            <button mat-flat-button color="primary" type="submit" [disabled]="form.invalid || submitting()">
              Register
            </button>
          </form>
        </mat-card-content>
        <mat-card-actions align="end">
          <a mat-button routerLink="/login">Already have an account?</a>
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
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  readonly submitting = signal(false);

  readonly form = this.fb.nonNullable.group(
    {
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, passwordStrengthValidator]],
      confirmPassword: ['', Validators.required],
    },
    { validators: passwordMatchValidator('password', 'confirmPassword') }
  );

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.auth.register(this.form.getRawValue()).subscribe({
      next: () => {
        this.submitting.set(false);
        this.snackBar.open('Account created successfully.', 'Dismiss', { duration: 4000 });
        void this.router.navigate(['/']);
      },
      error: (err) => {
        this.submitting.set(false);
        this.snackBar.open(extractApiErrorMessage(err, 'Registration failed.'), 'Dismiss', {
          duration: 5000,
        });
      },
    });
  }
}
