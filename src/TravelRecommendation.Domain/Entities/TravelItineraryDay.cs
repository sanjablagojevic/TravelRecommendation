using TravelRecommendation.Domain.Common;

namespace TravelRecommendation.Domain.Entities;

public class TravelItineraryDay : BaseEntity
{
    public int TravelItineraryId { get; set; }
    public int DayNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }

    public TravelItinerary TravelItinerary { get; set; } = null!;
    public ICollection<TravelItineraryItem> Items { get; set; } = new List<TravelItineraryItem>();
}
