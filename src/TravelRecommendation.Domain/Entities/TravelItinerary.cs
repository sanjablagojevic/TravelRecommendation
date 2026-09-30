using TravelRecommendation.Domain.Common;

namespace TravelRecommendation.Domain.Entities;

public class TravelItinerary : BaseEntity
{
    public int UserId { get; set; }
    public int DestinationId { get; set; }
    public int? UserPreferenceId { get; set; }
    public int? RecommendationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? AiModel { get; set; }

    public User User { get; set; } = null!;
    public Destination Destination { get; set; } = null!;
    public UserPreference? UserPreference { get; set; }
    public Recommendation? Recommendation { get; set; }
    public ICollection<TravelItineraryDay> Days { get; set; } = new List<TravelItineraryDay>();
}
