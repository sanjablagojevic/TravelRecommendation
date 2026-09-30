namespace TravelRecommendation.Application.DTOs.Recommendations;

public class RecommendationHistoryDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserPreferenceId { get; set; }
    public string? TopDestination { get; set; }
    public decimal? TopScore { get; set; }
    public int NumberOfResults { get; set; }
}
