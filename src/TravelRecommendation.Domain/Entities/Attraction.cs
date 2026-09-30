using TravelRecommendation.Domain.Common;

namespace TravelRecommendation.Domain.Entities;

public class Attraction : BaseEntity
{
    public int DestinationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? ImageUrl { get; set; }

    public Destination Destination { get; set; } = null!;
}
