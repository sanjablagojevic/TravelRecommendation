using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.DTOs.Ai;

namespace TravelRecommendation.Application.Ai;

public static class AiPreferenceOutputValidator
{
    public static void ValidateRequest(AiPreferenceRequestDto request)
    {
        var errors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            Add(errors, "Message", "Message is required.");
        }
        else
        {
            var trimmed = request.Message.Trim();
            if (trimmed.Length < 10)
            {
                Add(errors, "Message", "Message must be at least 10 characters.");
            }

            if (trimmed.Length > 2000)
            {
                Add(errors, "Message", "Message must be at most 2000 characters.");
            }
        }

        ThrowIfErrors(errors);
    }

    public static AiPreferenceExtractionDto ValidateAndNormalize(AiPreferenceExtractionDto raw)
    {
        var errors = new Dictionary<string, List<string>>();

        if (raw.Budget is <= 0)
        {
            Add(errors, "Budget", "Budget must be greater than 0 when provided.");
        }

        if (raw.Duration is < 1 or > 60)
        {
            Add(errors, "Duration", "Duration must be between 1 and 60 days when provided.");
        }

        if (raw.MinTemperature.HasValue && raw.MaxTemperature.HasValue
            && raw.MinTemperature > raw.MaxTemperature)
        {
            Add(errors, "MinTemperature", "MinTemperature must be less than or equal to MaxTemperature.");
        }

        var currency = SupportedTravelTaxonomy.NormalizeCurrency(raw.BudgetCurrency);
        if (!string.IsNullOrWhiteSpace(raw.BudgetCurrency) && currency is null)
        {
            Add(errors, "BudgetCurrency", "BudgetCurrency must be one of: EUR, BAM, USD, GBP.");
        }

        var tripType = SupportedTravelTaxonomy.NormalizeTripType(raw.TripType);
        if (!string.IsNullOrWhiteSpace(raw.TripType) && tripType is null)
        {
            // Treat unsupported trip type as unmapped rather than hard-failing the whole extraction.
            raw.UnmappedPreferences.Add($"TripType:{raw.TripType.Trim()}");
            tripType = null;
        }

        var normalizedInterests = new List<string>();
        var unmapped = new List<string>(
            raw.UnmappedPreferences
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase));

        foreach (var interest in raw.Interests.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            var normalized = SupportedTravelTaxonomy.NormalizeInterest(interest);
            if (normalized is null)
            {
                if (!unmapped.Contains(interest.Trim(), StringComparer.OrdinalIgnoreCase))
                {
                    unmapped.Add(interest.Trim());
                }
            }
            else if (!normalizedInterests.Contains(normalized, StringComparer.OrdinalIgnoreCase))
            {
                normalizedInterests.Add(normalized);
            }
        }

        ThrowIfErrors(errors);

        return new AiPreferenceExtractionDto
        {
            Budget = raw.Budget,
            BudgetCurrency = currency,
            Duration = raw.Duration,
            TripType = tripType,
            TravelPeriod = string.IsNullOrWhiteSpace(raw.TravelPeriod) ? null : raw.TravelPeriod.Trim(),
            PreferredClimate = string.IsNullOrWhiteSpace(raw.PreferredClimate) ? null : raw.PreferredClimate.Trim(),
            MinTemperature = raw.MinTemperature,
            MaxTemperature = raw.MaxTemperature,
            Interests = normalizedInterests,
            UnmappedPreferences = unmapped,
            AdditionalRequirements = string.IsNullOrWhiteSpace(raw.AdditionalRequirements)
                ? null
                : raw.AdditionalRequirements.Trim()
        };
    }

    public static void ValidateCreatePreference(CreatePreferenceFromAiRequestDto request)
    {
        var errors = new Dictionary<string, List<string>>();

        if (request.Budget is <= 0)
        {
            Add(errors, "Budget", "Budget must be greater than 0 when provided.");
        }

        if (request.Duration is < 1 or > 60)
        {
            Add(errors, "Duration", "Duration must be between 1 and 60 days when provided.");
        }

        if (request.MinTemperature.HasValue && request.MaxTemperature.HasValue
            && request.MinTemperature > request.MaxTemperature)
        {
            Add(errors, "MinTemperature", "MinTemperature must be less than or equal to MaxTemperature.");
        }

        if (request.Budget.HasValue)
        {
            var currency = SupportedTravelTaxonomy.NormalizeCurrency(request.BudgetCurrency);
            if (currency is null)
            {
                Add(errors, "BudgetCurrency", "BudgetCurrency is required and must be EUR when creating a preference with a budget.");
            }
            else if (!string.Equals(currency, "EUR", StringComparison.OrdinalIgnoreCase))
            {
                Add(
                    errors,
                    "BudgetCurrency",
                    "Only EUR budgets can be saved until currency conversion is implemented. Convert/confirm the amount in EUR first.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.TripType)
            && !SupportedTravelTaxonomy.IsSupportedTripType(request.TripType))
        {
            Add(errors, "TripType", "TripType is not supported.");
        }

        ThrowIfErrors(errors);
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

    private static void ThrowIfErrors(Dictionary<string, List<string>> errors)
    {
        if (errors.Count == 0)
        {
            return;
        }

        throw new ValidationException(errors.ToDictionary(x => x.Key, x => x.Value.ToArray()));
    }
}
