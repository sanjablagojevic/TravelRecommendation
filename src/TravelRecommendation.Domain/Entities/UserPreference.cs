using TravelRecommendation.Domain.Common;

namespace TravelRecommendation.Domain.Entities;

public class UserPreference : BaseEntity
{
    public int UserId { get; set; }
    public decimal? Budget { get; set; }
    public int? Duration { get; set; }
    public string? TripType { get; set; }
    public string? TravelPeriod { get; set; }
    public string? PreferredClimate { get; set; }
    public decimal? MinTemperature { get; set; }
    public decimal? MaxTemperature { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<UserPreferenceInterest> UserPreferenceInterests { get; set; } = new List<UserPreferenceInterest>();
    public ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
    public ICollection<TravelItinerary> TravelItineraries { get; set; } = new List<TravelItinerary>();
}
