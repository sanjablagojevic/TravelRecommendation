using System.Text.RegularExpressions;
using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.DTOs.Auth;
using TravelRecommendation.Application.DTOs.Users;

namespace TravelRecommendation.Application.Validators;

public static class AuthValidators
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex PasswordRegex = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
        RegexOptions.Compiled);

    public static void ValidateRegister(RegisterRequestDto request)
    {
        var errors = new Dictionary<string, List<string>>();

        ValidateRequiredName(errors, nameof(request.FirstName), request.FirstName);
        ValidateRequiredName(errors, nameof(request.LastName), request.LastName);
        ValidateEmail(errors, request.Email);
        ValidatePassword(errors, nameof(request.Password), request.Password);

        if (string.IsNullOrWhiteSpace(request.ConfirmPassword))
        {
            AddError(errors, nameof(request.ConfirmPassword), "Confirm password is required.");
        }
        else if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            AddError(errors, nameof(request.ConfirmPassword), "Passwords do not match.");
        }

        ThrowIfErrors(errors);
    }

    public static void ValidateLogin(LoginRequestDto request)
    {
        var errors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            AddError(errors, nameof(request.Email), "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            AddError(errors, nameof(request.Password), "Password is required.");
        }

        ThrowIfErrors(errors);
    }

    public static void ValidateUpdateProfile(UpdateProfileRequestDto request)
    {
        var errors = new Dictionary<string, List<string>>();

        ValidateRequiredName(errors, nameof(request.FirstName), request.FirstName);
        ValidateRequiredName(errors, nameof(request.LastName), request.LastName);

        ThrowIfErrors(errors);
    }

    public static void ValidateChangePassword(ChangePasswordRequestDto request)
    {
        var errors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            AddError(errors, nameof(request.CurrentPassword), "Current password is required.");
        }

        ValidatePassword(errors, nameof(request.NewPassword), request.NewPassword);

        if (string.IsNullOrWhiteSpace(request.ConfirmNewPassword))
        {
            AddError(errors, nameof(request.ConfirmNewPassword), "Confirm new password is required.");
        }
        else if (!string.Equals(request.NewPassword, request.ConfirmNewPassword, StringComparison.Ordinal))
        {
            AddError(errors, nameof(request.ConfirmNewPassword), "Passwords do not match.");
        }

        ThrowIfErrors(errors);
    }

    public static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static void ValidateRequiredName(Dictionary<string, List<string>> errors, string field, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            AddError(errors, field, $"{field} is required.");
            return;
        }

        if (value.Trim().Length > 150)
        {
            AddError(errors, field, $"{field} must be at most 150 characters.");
        }
    }

    private static void ValidateEmail(Dictionary<string, List<string>> errors, string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            AddError(errors, nameof(RegisterRequestDto.Email), "Email is required.");
            return;
        }

        if (email.Trim().Length > 256)
        {
            AddError(errors, nameof(RegisterRequestDto.Email), "Email must be at most 256 characters.");
            return;
        }

        if (!EmailRegex.IsMatch(email.Trim()))
        {
            AddError(errors, nameof(RegisterRequestDto.Email), "Email format is invalid.");
        }
    }

    private static void ValidatePassword(Dictionary<string, List<string>> errors, string field, string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            AddError(errors, field, "Password is required.");
            return;
        }

        if (!PasswordRegex.IsMatch(password))
        {
            AddError(
                errors,
                field,
                "Password must be at least 8 characters and contain at least one uppercase letter, one lowercase letter, and one digit.");
        }
    }

    private static void AddError(Dictionary<string, List<string>> errors, string field, string message)
    {
        if (!errors.TryGetValue(field, out var list))
        {
            list = [];
            errors[field] = list;
        }

        list.Add(message);
    }

    private static void ThrowIfErrors(Dictionary<string, List<string>> errors)
    {
        if (errors.Count == 0)
        {
            return;
        }

        throw new ValidationException(
            errors.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.ToArray()));
    }
}
