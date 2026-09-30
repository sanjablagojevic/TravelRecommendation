namespace TravelRecommendation.Domain.Entities;

public class DestinationInterest
{
    public int DestinationId { get; set; }
    public int InterestId { get; set; }

    public Destination Destination { get; set; } = null!;
    public Interest Interest { get; set; } = null!;
}
