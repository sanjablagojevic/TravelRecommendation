using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.DTOs.UserPreferences;

namespace TravelRecommendation.Application.Validators;

public static class PreferenceValidators
{
    public static List<int> ValidateAndNormalize(
        decimal? budget,
        int? duration,
        string? tripType,
        string? travelPeriod,
        string? preferredClimate,
        decimal? minTemperature,
        decimal? maxTemperature,
        IEnumerable<int>? interestIds)
    {
        var errors = new Dictionary<string, List<string>>();

        if (budget is <= 0)
        {
            Add(errors, "Budget", "Budget must be greater than 0.");
        }

        if (duration is < 1 or > 60)
        {
            Add(errors, "Duration", "Duration must be between 1 and 60 days.");
        }

        if (minTemperature is < -50 or > 60)
        {
            Add(errors, "MinTemperature", "MinTemperature must be between -50 and 60.");
        }

        if (maxTemperature is < -50 or > 60)
        {
            Add(errors, "MaxTemperature", "MaxTemperature must be between -50 and 60.");
        }

        if (minTemperature.HasValue && maxTemperature.HasValue && minTemperature > maxTemperature)
        {
            Add(errors, "MinTemperature", "MinTemperature must be less than or equal to MaxTemperature.");
        }

        ValidateOptionalLength(errors, "TripType", tripType, 100);
        ValidateOptionalLength(errors, "TravelPeriod", travelPeriod, 100);
        ValidateOptionalLength(errors, "PreferredClimate", preferredClimate, 100);

        var normalizedInterestIds = (interestIds ?? [])
            .Distinct()
            .ToList();

        if (errors.Count > 0)
        {
            throw new ValidationException(errors.ToDictionary(x => x.Key, x => x.Value.ToArray()));
        }

        return normalizedInterestIds;
    }

    public static List<int> ValidateCreate(CreateUserPreferenceRequestDto request)
    {
        return ValidateAndNormalize(
            request.Budget,
            request.Duration,
            request.TripType,
            request.TravelPeriod,
            request.PreferredClimate,
            request.MinTemperature,
            request.MaxTemperature,
            request.InterestIds);
    }

    public static List<int> ValidateUpdate(UpdateUserPreferenceRequestDto request)
    {
        return ValidateAndNormalize(
            request.Budget,
            request.Duration,
            request.TripType,
            request.TravelPeriod,
            request.PreferredClimate,
            request.MinTemperature,
            request.MaxTemperature,
            request.InterestIds);
    }

    private static void ValidateOptionalLength(
        Dictionary<string, List<string>> errors,
        string field,
        string? value,
        int maxLength)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Trim().Length > maxLength)
        {
            Add(errors, field, $"{field} must be at most {maxLength} characters.");
        }
    }

    private static void Add(Dictionary<string, List<string>> errors, string field, string message)
    {
        if (!errors.TryGetValue(field, out var list))
        {
            list = [];
            errors[field] = list;
        }

        list.Add(message);
    }
}
