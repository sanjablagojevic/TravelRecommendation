using TravelRecommendation.Domain.Common;
using TravelRecommendation.Domain.Enums;

namespace TravelRecommendation.Domain.Entities;

public class TravelItineraryItem : BaseEntity
{
    public int TravelItineraryDayId { get; set; }
    public int Order { get; set; }
    public string TimeOfDay { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ItineraryItemType ItemType { get; set; } = ItineraryItemType.General;
    public int? AttractionId { get; set; }
    public int? ActivityId { get; set; }
    public string? Location { get; set; }

    public TravelItineraryDay TravelItineraryDay { get; set; } = null!;
    public Attraction? Attraction { get; set; }
    public Activity? Activity { get; set; }
}
