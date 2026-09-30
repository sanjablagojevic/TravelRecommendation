using TravelRecommendation.Domain.Common;

namespace TravelRecommendation.Domain.Entities;

public class Destination : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal AverageDailyCost { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal Popularity { get; set; }
    public string? ImageUrl { get; set; }
    public string? Climate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<DestinationCategory> DestinationCategories { get; set; } = new List<DestinationCategory>();
    public ICollection<DestinationInterest> DestinationInterests { get; set; } = new List<DestinationInterest>();
    public ICollection<Attraction> Attractions { get; set; } = new List<Attraction>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<RecommendationItem> RecommendationItems { get; set; } = new List<RecommendationItem>();
    public ICollection<TravelHistory> TravelHistories { get; set; } = new List<TravelHistory>();
    public ICollection<TravelItinerary> TravelItineraries { get; set; } = new List<TravelItinerary>();
}
