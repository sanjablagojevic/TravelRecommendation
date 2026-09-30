using TravelRecommendation.Application.DTOs.Recommendations;

namespace TravelRecommendation.Application.Recommendations;

public sealed class DestinationScoreResult
{
    public int DestinationId { get; init; }
    public string DestinationName { get; init; } = string.Empty;
    public decimal FinalScore { get; init; }
    public decimal? EstimatedCost { get; init; }
    public IReadOnlyList<string> MatchedInterests { get; init; } = [];
    public IReadOnlyList<string> MissingInterests { get; init; } = [];
    public RecommendationScoreBreakdownDto Breakdown { get; init; } = null!;
    public string Explanation { get; init; } = string.Empty;
}
