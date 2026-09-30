namespace TravelRecommendation.Application.Recommendations;

/// <summary>
/// Scoring weights for the deterministic recommendation engine.
/// Assumption: Budget and AverageDailyCost are both in EUR (no currency conversion yet).
/// </summary>
public sealed class RecommendationWeights
{
    public decimal Interest { get; init; } = 0.30m;
    public decimal Budget { get; init; } = 0.25m;
    public decimal TripType { get; init; } = 0.15m;
    public decimal Climate { get; init; } = 0.15m;
    public decimal Popularity { get; init; } = 0.15m;

    public decimal Total => Interest + Budget + TripType + Climate + Popularity;

    public void EnsureValid()
    {
        if (Total != 1.0m)
        {
            throw new InvalidOperationException(
                $"RecommendationWeights must sum to 1.0. Current total: {Total}.");
        }
    }
}
