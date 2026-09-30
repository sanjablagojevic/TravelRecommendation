namespace TravelRecommendation.Application.DTOs.Ai;

public class AiPreferenceExtractionResponseDto
{
    public decimal? Budget { get; set; }
    public string? BudgetCurrency { get; set; }
    public int? Duration { get; set; }
    public string? TripType { get; set; }
    public string? TravelPeriod { get; set; }
    public string? PreferredClimate { get; set; }
    public decimal? MinTemperature { get; set; }
    public decimal? MaxTemperature { get; set; }
    public IReadOnlyList<AiMappedInterestDto> Interests { get; set; } = [];
    public IReadOnlyList<string> UnmappedPreferences { get; set; } = [];
    public string? AdditionalRequirements { get; set; }

    /// <summary>
    /// True when a budget is present but currency is not EUR.
    /// Recommendation Engine currently expects EUR — no invented FX rates are applied.
    /// </summary>
    public bool RequiresCurrencyConversion { get; set; }

    public string? CurrencyNote { get; set; }
}
