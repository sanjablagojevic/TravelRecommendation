namespace TravelRecommendation.Application.DTOs.Itineraries;

public class ItineraryDestinationSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

public class TravelItineraryItemDto
{
    public int Order { get; set; }
    public string TimeOfDay { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ItemType { get; set; } = "General";
    public int? AttractionId { get; set; }
    public string? AttractionName { get; set; }
    public string? AttractionImageUrl { get; set; }
    public int? ActivityId { get; set; }
    public string? ActivityName { get; set; }
    public decimal? ActivityPrice { get; set; }
    public string? ActivityCurrency { get; set; }
    public string? Location { get; set; }
}

public class TravelItineraryDayDto
{
    public int DayNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public IReadOnlyList<TravelItineraryItemDto> Items { get; set; } = [];
}

public class TravelItineraryDto
{
    public int Id { get; set; }
    public ItineraryDestinationSummaryDto Destination { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<TravelItineraryDayDto> Days { get; set; } = [];
}

public class ItineraryListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int DestinationId { get; set; }
    public string DestinationName { get; set; } = string.Empty;
    public string? DestinationImageUrl { get; set; }
    public int DurationDays { get; set; }
    public DateTime CreatedAt { get; set; }
}
