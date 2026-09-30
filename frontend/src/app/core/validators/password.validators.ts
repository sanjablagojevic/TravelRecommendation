import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

const PASSWORD_PATTERN = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/;

export const passwordStrengthValidator: ValidatorFn = (
  control: AbstractControl
): ValidationErrors | null => {
  const value = control.value as string;
  if (!value) {
    return null;
  }
  return PASSWORD_PATTERN.test(value)
    ? null
    : {
        passwordStrength:
          'Password must be at least 8 characters and include uppercase, lowercase, and a digit.',
      };
};

export function passwordMatchValidator(
  passwordField: string,
  confirmField: string
): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const password = group.get(passwordField)?.value;
    const confirm = group.get(confirmField)?.value;
    if (!password || !confirm) {
      return null;
    }
    return password === confirm ? null : { passwordMismatch: true };
  };
}
