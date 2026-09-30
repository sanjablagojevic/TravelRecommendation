import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { UserService } from '../../../core/services/user.service';
import { UserProfile } from '../../../core/models/user.models';
import {
  passwordMatchValidator,
  passwordStrengthValidator,
} from '../../../core/validators/password.validators';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { extractApiErrorMessage } from '../../../core/utils/api-error.util';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-profile-page',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    MatCardModule,
    MatTabsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    LoadingSpinnerComponent,
  ],
  template: `
    <header class="page-header">
      <h1>Profile</h1>
      <p>Manage your account details and password.</p>
    </header>

    @if (loading()) {
      <app-loading-spinner message="Loading profile..." />
    } @else if (profile(); as user) {
      <mat-card class="summary">
        <mat-card-content>
          <p><strong>Email:</strong> {{ user.email }}</p>
          <p><strong>Role:</strong> {{ user.role }}</p>
          <p><strong>Member since:</strong> {{ user.createdAt | date: 'mediumDate' }}</p>
        </mat-card-content>
      </mat-card>

      <mat-tab-group>
        <mat-tab label="Details">
          <form class="tab-form" [formGroup]="profileForm" (ngSubmit)="saveProfile()">
            <mat-form-field appearance="outline" class="full">
              <mat-label>First name</mat-label>
              <input matInput formControlName="firstName" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="full">
              <mat-label>Last name</mat-label>
              <input matInput formControlName="lastName" />
            </mat-form-field>
            <button mat-flat-button color="primary" type="submit" [disabled]="profileForm.invalid || savingProfile()">
              Save changes
            </button>
          </form>
        </mat-tab>
        <mat-tab label="Password">
          <form class="tab-form" [formGroup]="passwordForm" (ngSubmit)="changePassword()">
            <mat-form-field appearance="outline" class="full">
              <mat-label>Current password</mat-label>
              <input matInput type="password" formControlName="currentPassword" autocomplete="current-password" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="full">
              <mat-label>New password</mat-label>
              <input matInput type="password" formControlName="newPassword" autocomplete="new-password" />
              @if (passwordForm.controls.newPassword.hasError('passwordStrength')) {
                <mat-error>{{ passwordForm.controls.newPassword.getError('passwordStrength') }}</mat-error>
              }
            </mat-form-field>
            <mat-form-field appearance="outline" class="full">
              <mat-label>Confirm new password</mat-label>
              <input matInput type="password" formControlName="confirmNewPassword" autocomplete="new-password" />
              @if (passwordForm.hasError('passwordMismatch')) {
                <mat-error>Passwords do not match</mat-error>
              }
            </mat-form-field>
            <button mat-flat-button color="primary" type="submit" [disabled]="passwordForm.invalid || savingPassword()">
              Update password
            </button>
          </form>
        </mat-tab>
      </mat-tab-group>
    }
  `,
  styles: `
    .page-header h1 {
      margin: 0 0 0.25rem;
    }

    .page-header p {
      margin: 0 0 1rem;
      color: var(--mat-sys-on-surface-variant);
    }

    .summary {
      margin-bottom: 1rem;
    }

    .tab-form {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
      padding: 1rem 0;
      max-width: 420px;
    }

    .full {
      width: 100%;
    }

    :host {
      display: block;
      width: min(900px, 100%);
      margin: 0 auto;
      padding: 1.5rem 1rem 2rem;
      box-sizing: border-box;
    }
  `,
})
export class ProfilePageComponent {
  private readonly userService = inject(UserService);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);
  private readonly snackBar = inject(MatSnackBar);

  readonly profile = signal<UserProfile | null>(null);
  readonly loading = signal(true);
  readonly savingProfile = signal(false);
  readonly savingPassword = signal(false);

  readonly profileForm = this.fb.nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
  });

  readonly passwordForm = this.fb.nonNullable.group(
    {
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, passwordStrengthValidator]],
      confirmNewPassword: ['', Validators.required],
    },
    { validators: passwordMatchValidator('newPassword', 'confirmNewPassword') }
  );

  constructor() {
    this.userService.getProfile().subscribe({
      next: (user) => {
        this.profile.set(user);
        this.profileForm.patchValue({
          firstName: user.firstName,
          lastName: user.lastName,
        });
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.snackBar.open(extractApiErrorMessage(err), 'Dismiss', { duration: 5000 });
      },
    });
  }

  saveProfile(): void {
    if (this.profileForm.invalid) {
      return;
    }

    this.savingProfile.set(true);
    this.userService.updateProfile(this.profileForm.getRawValue()).subscribe({
      next: (user) => {
        this.profile.set(user);
        this.savingProfile.set(false);
        this.snackBar.open('Profile updated', 'Dismiss', { duration: 3000 });
        void this.auth.initialize().subscribe();
      },
      error: (err) => {
        this.savingProfile.set(false);
        this.snackBar.open(extractApiErrorMessage(err), 'Dismiss', { duration: 5000 });
      },
    });
  }

  changePassword(): void {
    if (this.passwordForm.invalid) {
      return;
    }

    this.savingPassword.set(true);
    this.userService.changePassword(this.passwordForm.getRawValue()).subscribe({
      next: () => {
        this.savingPassword.set(false);
        this.passwordForm.reset();
        this.snackBar.open('Password updated', 'Dismiss', { duration: 3000 });
      },
      error: (err) => {
        this.savingPassword.set(false);
        this.snackBar.open(extractApiErrorMessage(err), 'Dismiss', { duration: 5000 });
      },
    });
  }
}
