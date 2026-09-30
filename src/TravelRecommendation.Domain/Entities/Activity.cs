using TravelRecommendation.Domain.Common;

namespace TravelRecommendation.Domain.Entities;

public class Activity : BaseEntity
{
    public int DestinationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Provider { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public string? ExternalUrl { get; set; }
    public string? Description { get; set; }

    public Destination Destination { get; set; } = null!;
}
