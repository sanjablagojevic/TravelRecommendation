namespace TravelRecommendation.Application.DTOs.Ai;

public class AiExplanationsResponseDto
{
    public int RecommendationId { get; set; }
    public bool UsedFallback { get; set; }
    public string? Warning { get; set; }
    public IReadOnlyList<AiExplanationItemDto> Items { get; set; } = [];
}
