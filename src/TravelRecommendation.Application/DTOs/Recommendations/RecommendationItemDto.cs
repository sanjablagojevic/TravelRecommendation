namespace TravelRecommendation.Application.DTOs.Recommendations;

public class RecommendationItemDto
{
    public int Rank { get; set; }
    public RecommendationDestinationDto Destination { get; set; } = null!;
    public decimal Score { get; set; }
    public decimal MatchPercentage { get; set; }
    public decimal? EstimatedCost { get; set; }
    public IReadOnlyList<string> MatchedInterests { get; set; } = [];
    public IReadOnlyList<string> MissingInterests { get; set; } = [];
    public string Explanation { get; set; } = string.Empty;
    public string? AiExplanation { get; set; }
    public RecommendationScoreBreakdownDto? ScoreBreakdown { get; set; }
}
