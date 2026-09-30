namespace TravelRecommendation.Application.DTOs.Weather;

public class CurrentWeatherDto
{
    public decimal Temperature { get; set; }
    public decimal FeelsLike { get; set; }
    public decimal? MinTemperature { get; set; }
    public decimal? MaxTemperature { get; set; }
    public int Humidity { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public decimal WindSpeed { get; set; }
    public DateTime ObservedAt { get; set; }
}

public class ForecastDayDto
{
    public DateOnly Date { get; set; }
    public decimal MinTemperature { get; set; }
    public decimal MaxTemperature { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Icon { get; set; }
}

public class WeatherResponseDto
{
    public int DestinationId { get; set; }
    public string DestinationName { get; set; } = string.Empty;
    public CurrentWeatherDto Current { get; set; } = null!;
    public IReadOnlyList<ForecastDayDto> Forecast { get; set; } = [];
    public DateTime FetchedAt { get; set; }
    public bool IsCached { get; set; }
    public bool IsStale { get; set; }
}
