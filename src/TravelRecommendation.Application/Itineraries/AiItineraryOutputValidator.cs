using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.DTOs.Itineraries;
using TravelRecommendation.Domain.Enums;

namespace TravelRecommendation.Application.Itineraries;

public static class AiItineraryOutputValidator
{
    private static readonly HashSet<string> AllowedTimes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Morning", "Afternoon", "Evening"
    };

    public static void ValidateRequest(GenerateItineraryRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.DestinationId <= 0)
        {
            errors["DestinationId"] = ["DestinationId is required."];
        }

        if (request.DurationDays < 1 || request.DurationDays > 14)
        {
            errors["DurationDays"] = ["DurationDays must be between 1 and 14."];
        }

        if (request.AdditionalRequest is { Length: > 1000 })
        {
            errors["AdditionalRequest"] = ["AdditionalRequest must be at most 1000 characters."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    public static AiGeneratedItineraryDto ValidateAndNormalize(
        AiGeneratedItineraryDto raw,
        int expectedDays,
        IReadOnlySet<int> validAttractionIds,
        IReadOnlySet<int> validActivityIds)
    {
        if (string.IsNullOrWhiteSpace(raw.Title))
        {
            throw InvalidAi("Itinerary title is missing.");
        }

        if (raw.Days is null || raw.Days.Count != expectedDays)
        {
            throw InvalidAi($"Expected exactly {expectedDays} days.");
        }

        var dayNumbers = raw.Days.Select(d => d.DayNumber).OrderBy(n => n).ToList();
        var expected = Enumerable.Range(1, expectedDays).ToList();
        if (!dayNumbers.SequenceEqual(expected))
        {
            throw InvalidAi("Day numbers must be unique and cover 1..DurationDays.");
        }

        var normalizedDays = new List<AiGeneratedItineraryDayDto>();

        foreach (var day in raw.Days.OrderBy(d => d.DayNumber))
        {
            if (string.IsNullOrWhiteSpace(day.Title))
            {
                throw InvalidAi($"Day {day.DayNumber} is missing a title.");
            }

            if (day.Items is null || day.Items.Count == 0)
            {
                throw InvalidAi($"Day {day.DayNumber} must contain at least one item.");
            }

            if (day.Items.Count > 6)
            {
                throw InvalidAi($"Day {day.DayNumber} has too many items.");
            }

            var orders = day.Items.Select(i => i.Order).OrderBy(o => o).ToList();
            var expectedOrders = Enumerable.Range(1, day.Items.Count).ToList();
            if (!orders.SequenceEqual(expectedOrders))
            {
                // Normalize order if contiguous after sort by provided order
                var sorted = day.Items.OrderBy(i => i.Order).ThenBy(i => i.Title).ToList();
                for (var i = 0; i < sorted.Count; i++)
                {
                    sorted[i].Order = i + 1;
                }

                day.Items = sorted;
            }

            var normalizedItems = new List<AiGeneratedItineraryItemDto>();
            foreach (var item in day.Items.OrderBy(i => i.Order))
            {
                normalizedItems.Add(NormalizeItem(item, day.DayNumber, validAttractionIds, validActivityIds));
            }

            normalizedDays.Add(new AiGeneratedItineraryDayDto
            {
                DayNumber = day.DayNumber,
                Title = day.Title.Trim(),
                Summary = string.IsNullOrWhiteSpace(day.Summary) ? null : day.Summary.Trim(),
                Items = normalizedItems
            });
        }

        return new AiGeneratedItineraryDto
        {
            Title = raw.Title.Trim().Length > 200 ? raw.Title.Trim()[..200] : raw.Title.Trim(),
            Days = normalizedDays
        };
    }

    private static AiGeneratedItineraryItemDto NormalizeItem(
        AiGeneratedItineraryItemDto item,
        int dayNumber,
        IReadOnlySet<int> validAttractionIds,
        IReadOnlySet<int> validActivityIds)
    {
        if (string.IsNullOrWhiteSpace(item.Title))
        {
            throw InvalidAi($"Day {dayNumber} has an item without a title.");
        }

        if (!AllowedTimes.Contains(item.TimeOfDay?.Trim() ?? string.Empty))
        {
            throw InvalidAi($"Day {dayNumber} has an invalid timeOfDay.");
        }

        var itemType = ParseItemType(item.ItemType);
        int? attractionId = item.AttractionId;
        int? activityId = item.ActivityId;

        switch (itemType)
        {
            case ItineraryItemType.Attraction:
                if (!attractionId.HasValue || !validAttractionIds.Contains(attractionId.Value))
                {
                    throw InvalidAi($"Day {dayNumber} references an invalid attractionId.");
                }

                activityId = null;
                break;
            case ItineraryItemType.Activity:
                if (!activityId.HasValue || !validActivityIds.Contains(activityId.Value))
                {
                    throw InvalidAi($"Day {dayNumber} references an invalid activityId.");
                }

                attractionId = null;
                break;
            default:
                if (attractionId.HasValue || activityId.HasValue)
                {
                    // General items must not carry foreign keys
                    attractionId = null;
                    activityId = null;
                }

                itemType = ItineraryItemType.General;
                break;
        }

        var timeRaw = item.TimeOfDay?.Trim() ?? string.Empty;
        var time = char.ToUpperInvariant(timeRaw[0]) + timeRaw[1..].ToLowerInvariant();

        return new AiGeneratedItineraryItemDto
        {
            Order = item.Order,
            TimeOfDay = time,
            Title = Truncate(item.Title.Trim(), 200),
            Description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim(),
            ItemType = itemType.ToString(),
            AttractionId = attractionId,
            ActivityId = activityId,
            Location = string.IsNullOrWhiteSpace(item.Location) ? null : Truncate(item.Location.Trim(), 300)
        };
    }

    private static ItineraryItemType ParseItemType(string? value)
    {
        if (Enum.TryParse<ItineraryItemType>(value, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        return ItineraryItemType.General;
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];

    private static Exception InvalidAi(string technicalReason) =>
        new InvalidOperationException(technicalReason);
}
