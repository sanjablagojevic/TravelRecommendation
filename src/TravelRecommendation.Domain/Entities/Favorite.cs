using TravelRecommendation.Domain.Common;

namespace TravelRecommendation.Domain.Entities;

public class Favorite : BaseEntity
{
    public int UserId { get; set; }
    public int DestinationId { get; set; }
    public DateTime AddedAt { get; set; }

    public User User { get; set; } = null!;
    public Destination Destination { get; set; } = null!;
}
