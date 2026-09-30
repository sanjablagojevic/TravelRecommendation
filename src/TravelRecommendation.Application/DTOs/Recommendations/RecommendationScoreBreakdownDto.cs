namespace TravelRecommendation.Application.DTOs.Recommendations;

public class ScoreComponentDto
{
    public string Name { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public decimal Weight { get; set; }
    public bool IsActive { get; set; }
    public decimal? WeightedValue { get; set; }
}

public class RecommendationScoreBreakdownDto
{
    public decimal? InterestScore { get; set; }
    public decimal? BudgetScore { get; set; }
    public decimal? TripTypeScore { get; set; }
    public decimal? ClimateScore { get; set; }
    public decimal? PopularityScore { get; set; }
    public decimal ActiveWeightTotal { get; set; }
    public IReadOnlyList<ScoreComponentDto> Components { get; set; } = [];
}
