import { HttpErrorResponse } from '@angular/common/http';
import { ApiErrorBody } from '../models/common.models';

export function extractApiErrorMessage(error: unknown, fallback = 'Something went wrong.'): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallback;
  }

  if (error.status === 403) {
    return "You don't have permission to perform this action.";
  }

  const body = error.error as ApiErrorBody | string | null;
  if (!body) {
    return fallback;
  }

  if (typeof body === 'string') {
    return body;
  }

  if (body.errors) {
    const messages = Object.values(body.errors).flat();
    if (messages.length > 0) {
      return messages.join(' ');
    }
  }

  return body.message ?? fallback;
}
