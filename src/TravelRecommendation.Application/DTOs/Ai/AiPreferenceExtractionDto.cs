namespace TravelRecommendation.Application.DTOs.Ai;

/// <summary>
/// Raw structured preferences extracted by the AI model before DB interest mapping.
/// </summary>
public class AiPreferenceExtractionDto
{
    public decimal? Budget { get; set; }
    public string? BudgetCurrency { get; set; }
    public int? Duration { get; set; }
    public string? TripType { get; set; }
    public string? TravelPeriod { get; set; }
    public string? PreferredClimate { get; set; }
    public decimal? MinTemperature { get; set; }
    public decimal? MaxTemperature { get; set; }
    public List<string> Interests { get; set; } = [];
    public List<string> UnmappedPreferences { get; set; } = [];
    public string? AdditionalRequirements { get; set; }
}
