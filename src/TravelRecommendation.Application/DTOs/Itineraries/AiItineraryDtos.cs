namespace TravelRecommendation.Application.DTOs.Itineraries;

/// <summary>Structured AI response before persistence validation.</summary>
public class AiGeneratedItineraryDto
{
    public string Title { get; set; } = string.Empty;
    public List<AiGeneratedItineraryDayDto> Days { get; set; } = [];
}

public class AiGeneratedItineraryDayDto
{
    public int DayNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public List<AiGeneratedItineraryItemDto> Items { get; set; } = [];
}

public class AiGeneratedItineraryItemDto
{
    public int Order { get; set; }
    public string TimeOfDay { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ItemType { get; set; } = "General";
    public int? AttractionId { get; set; }
    public int? ActivityId { get; set; }
    public string? Location { get; set; }
}

public class AiItineraryGenerationContext
{
    public int DestinationId { get; set; }
    public string DestinationName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Climate { get; set; }
    public IReadOnlyList<string> Categories { get; set; } = [];
    public IReadOnlyList<string> Interests { get; set; } = [];
    public IReadOnlyList<AiItineraryCatalogItem> Attractions { get; set; } = [];
    public IReadOnlyList<AiItineraryCatalogItem> Activities { get; set; } = [];
    public int DurationDays { get; set; }
    public string? TripType { get; set; }
    public string? TravelPeriod { get; set; }
    public string? PreferredClimate { get; set; }
    public IReadOnlyList<string> PreferenceInterests { get; set; } = [];
    public string? AdditionalRequest { get; set; }
}

public class AiItineraryCatalogItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
}
