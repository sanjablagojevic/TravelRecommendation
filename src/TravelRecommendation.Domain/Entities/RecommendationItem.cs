using TravelRecommendation.Domain.Common;

namespace TravelRecommendation.Domain.Entities;

public class RecommendationItem : BaseEntity
{
    public int RecommendationId { get; set; }
    public int DestinationId { get; set; }
    public decimal Score { get; set; }
    public int Rank { get; set; }
    public string? Explanation { get; set; }
    public string? AiExplanation { get; set; }
    public decimal? EstimatedCost { get; set; }

    public Recommendation Recommendation { get; set; } = null!;
    public Destination Destination { get; set; } = null!;
}
