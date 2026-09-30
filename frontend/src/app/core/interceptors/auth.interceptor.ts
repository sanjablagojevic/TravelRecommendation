import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  let outgoing = req;
  if (req.url.startsWith(environment.apiUrl)) {
    const token = auth.getToken();
    if (token) {
      outgoing = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` },
      });
    }
  }

  return next(outgoing).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        const isAuthEndpoint =
          req.url.includes('/auth/login') || req.url.includes('/auth/register');
        if (!isAuthEndpoint) {
          auth.clearSession();
          void router.navigate(['/login']);
        }
      }
      return throwError(() => error);
    })
  );
};
