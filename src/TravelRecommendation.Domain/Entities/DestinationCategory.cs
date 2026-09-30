namespace TravelRecommendation.Domain.Entities;

public class DestinationCategory
{
    public int DestinationId { get; set; }
    public int CategoryId { get; set; }

    public Destination Destination { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
