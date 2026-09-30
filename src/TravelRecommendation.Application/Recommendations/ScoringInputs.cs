namespace TravelRecommendation.Application.Recommendations;

public sealed class PreferenceScoringInput
{
    public decimal? Budget { get; init; }
    public int? Duration { get; init; }
    public string? TripType { get; init; }
    public string? PreferredClimate { get; init; }
    public IReadOnlyList<string> InterestNames { get; init; } = [];
}

public sealed class DestinationScoringInput
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public decimal AverageDailyCost { get; init; }
    public decimal Popularity { get; init; }
    public string? Climate { get; init; }
    public string? ImageUrl { get; init; }
    public IReadOnlyList<string> InterestNames { get; init; } = [];
    public IReadOnlyList<string> CategoryNames { get; init; } = [];
}
