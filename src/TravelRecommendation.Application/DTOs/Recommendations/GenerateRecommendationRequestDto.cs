namespace TravelRecommendation.Application.DTOs.Recommendations;

public class GenerateRecommendationRequestDto
{
    public int UserPreferenceId { get; set; }
    public int? TopCount { get; set; }
}
