using TravelRecommendation.Domain.Common;

namespace TravelRecommendation.Domain.Entities;

public class Recommendation : BaseEntity
{
    public int? UserId { get; set; }
    public int UserPreferenceId { get; set; }
    public string? OriginalQuery { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
    public UserPreference UserPreference { get; set; } = null!;
    public ICollection<RecommendationItem> RecommendationItems { get; set; } = new List<RecommendationItem>();
    public ICollection<TravelItinerary> TravelItineraries { get; set; } = new List<TravelItinerary>();
}
