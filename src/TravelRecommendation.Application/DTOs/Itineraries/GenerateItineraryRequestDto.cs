namespace TravelRecommendation.Application.DTOs.Itineraries;

public class GenerateItineraryRequestDto
{
    public int DestinationId { get; set; }
    public int DurationDays { get; set; }
    public int? UserPreferenceId { get; set; }
    public int? RecommendationId { get; set; }
    public string? AdditionalRequest { get; set; }
}
