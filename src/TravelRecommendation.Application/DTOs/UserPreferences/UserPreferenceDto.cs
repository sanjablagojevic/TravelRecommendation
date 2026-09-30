namespace TravelRecommendation.Application.DTOs.UserPreferences;

public class UserPreferenceDto
{
    public int Id { get; set; }
    public decimal? Budget { get; set; }
    public int? Duration { get; set; }
    public string? TripType { get; set; }
    public string? TravelPeriod { get; set; }
    public string? PreferredClimate { get; set; }
    public decimal? MinTemperature { get; set; }
    public decimal? MaxTemperature { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<PreferenceInterestDto> Interests { get; set; } = [];
}
