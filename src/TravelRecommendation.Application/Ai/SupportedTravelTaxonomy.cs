namespace TravelRecommendation.Application.Ai;

public static class SupportedTravelTaxonomy
{
    public static readonly IReadOnlyList<string> Interests =
    [
        "Beach",
        "Nature",
        "Culture",
        "History",
        "Food",
        "Nightlife",
        "Adventure",
        "Relaxation",
        "Mountains",
        "Shopping"
    ];

    public static readonly IReadOnlyList<string> TripTypes =
    [
        "Beach",
        "City Break",
        "Nature",
        "Culture",
        "Adventure",
        "Relaxation",
        "Mountains"
    ];

    public static readonly IReadOnlyList<string> Currencies =
    [
        "EUR",
        "BAM",
        "USD",
        "GBP"
    ];

    public static bool IsSupportedInterest(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && Interests.Any(i => string.Equals(i, value.Trim(), StringComparison.OrdinalIgnoreCase));

    public static bool IsSupportedTripType(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && TripTypes.Any(t => string.Equals(t, value.Trim(), StringComparison.OrdinalIgnoreCase));

    public static bool IsSupportedCurrency(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && Currencies.Any(c => string.Equals(c, value.Trim(), StringComparison.OrdinalIgnoreCase));

    public static string? NormalizeInterest(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Interests.FirstOrDefault(i =>
            string.Equals(i, value.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public static string? NormalizeTripType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return TripTypes.FirstOrDefault(t =>
            string.Equals(t, value.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public static string? NormalizeCurrency(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim().ToUpperInvariant();
        if (trimmed is "KM" or "KONVERTIBILNA MARKA")
        {
            return "BAM";
        }

        return Currencies.FirstOrDefault(c =>
            string.Equals(c, trimmed, StringComparison.OrdinalIgnoreCase));
    }
}
