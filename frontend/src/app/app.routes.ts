import { Routes } from '@angular/router';
import { adminGuard } from './core/guards/admin.guard';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { unsavedChangesGuard } from './core/guards/unsaved-changes.guard';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/register/register.component').then((m) => m.RegisterComponent),
  },
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () =>
          import('./features/home/home.component').then((m) => m.HomeComponent),
      },
      {
        path: 'destinations',
        loadComponent: () =>
          import('./features/destinations/destination-list/destination-list.component').then(
            (m) => m.DestinationListComponent
          ),
      },
      {
        path: 'destinations/:id',
        loadComponent: () =>
          import('./features/destinations/destination-detail/destination-detail.component').then(
            (m) => m.DestinationDetailComponent
          ),
      },
      {
        path: 'plan-trip',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/plan-trip/plan-trip-landing.component').then(
            (m) => m.PlanTripLandingComponent
          ),
      },
      {
        path: 'plan-trip/manual',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/plan-trip/manual-questionnaire.component').then(
            (m) => m.ManualQuestionnaireComponent
          ),
      },
      {
        path: 'plan-trip/ai',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/plan-trip/ai-input.component').then((m) => m.AiInputComponent),
      },
      {
        path: 'plan-trip/confirm',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/plan-trip/confirm-preferences.component').then(
            (m) => m.ConfirmPreferencesComponent
          ),
      },
      {
        path: 'recommendations/:id',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/recommendations/recommendation-results.component').then(
            (m) => m.RecommendationResultsComponent
          ),
      },
      {
        path: 'recommendation-history',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/recommendations/recommendation-history.component').then(
            (m) => m.RecommendationHistoryComponent
          ),
      },
      {
        path: 'itineraries',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/itineraries/itinerary-list.component').then(
            (m) => m.ItineraryListComponent
          ),
      },
      {
        path: 'itineraries/:id',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/itineraries/itinerary-detail.component').then(
            (m) => m.ItineraryDetailComponent
          ),
      },
      {
        path: 'favorites',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/favorites/favorites-page/favorites-page.component').then(
            (m) => m.FavoritesPageComponent
          ),
      },
      {
        path: 'profile',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/profile/profile-page/profile-page.component').then(
            (m) => m.ProfilePageComponent
          ),
      },
      {
        path: 'admin',
        canActivate: [authGuard, adminGuard],
        loadComponent: () =>
          import('./features/admin/admin-layout.component').then((m) => m.AdminLayoutComponent),
        children: [
          {
            path: '',
            pathMatch: 'full',
            loadComponent: () =>
              import('./features/admin/admin-dashboard.component').then(
                (m) => m.AdminDashboardComponent
              ),
          },
          {
            path: 'destinations',
            loadComponent: () =>
              import(
                './features/admin/destinations/admin-destinations-list.component'
              ).then((m) => m.AdminDestinationsListComponent),
          },
          {
            path: 'destinations/new',
            canDeactivate: [unsavedChangesGuard],
            loadComponent: () =>
              import('./features/admin/destinations/admin-destination-form.component').then(
                (m) => m.AdminDestinationFormComponent
              ),
          },
          {
            path: 'destinations/:id/edit',
            canDeactivate: [unsavedChangesGuard],
            loadComponent: () =>
              import('./features/admin/destinations/admin-destination-form.component').then(
                (m) => m.AdminDestinationFormComponent
              ),
          },
          {
            path: 'destinations/:id/attractions',
            redirectTo: 'destinations/:id/edit',
          },
          {
            path: 'destinations/:id/activities',
            redirectTo: 'destinations/:id/edit',
          },
          {
            path: 'categories',
            loadComponent: () =>
              import('./features/admin/categories/admin-categories.component').then(
                (m) => m.AdminCategoriesComponent
              ),
          },
          {
            path: 'interests',
            loadComponent: () =>
              import('./features/admin/interests/admin-interests.component').then(
                (m) => m.AdminInterestsComponent
              ),
          },
        ],
      },
      {
        path: '**',
        loadComponent: () =>
          import('./features/not-found/not-found.component').then((m) => m.NotFoundComponent),
      },
    ],
  },
];
