using TravelRecommendation.Domain.Common;

namespace TravelRecommendation.Domain.Entities;

public class TravelHistory : BaseEntity
{
    public int UserId { get; set; }
    public int DestinationId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public Destination Destination { get; set; } = null!;
}
