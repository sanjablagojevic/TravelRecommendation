namespace TravelRecommendation.Application.DTOs.UserPreferences;

public class CreateUserPreferenceRequestDto
{
    public decimal? Budget { get; set; }
    public int? Duration { get; set; }
    public string? TripType { get; set; }
    public string? TravelPeriod { get; set; }
    public string? PreferredClimate { get; set; }
    public decimal? MinTemperature { get; set; }
    public decimal? MaxTemperature { get; set; }
    public List<int> InterestIds { get; set; } = [];
}
