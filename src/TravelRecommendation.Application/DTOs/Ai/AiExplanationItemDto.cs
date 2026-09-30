namespace TravelRecommendation.Application.DTOs.Ai;

public class AiExplanationItemDto
{
    public int RecommendationItemId { get; set; }
    public int DestinationId { get; set; }
    public int Rank { get; set; }
    public decimal Score { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? DeterministicExplanation { get; set; }
    public string? AiExplanation { get; set; }
}
