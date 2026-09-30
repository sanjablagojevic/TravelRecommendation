namespace TravelRecommendation.Application.DTOs.Recommendations;

public class RecommendationDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserPreferenceId { get; set; }
    public IReadOnlyList<RecommendationItemDto> Items { get; set; } = [];
}
