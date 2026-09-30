namespace TravelRecommendation.Application.Settings;

public class WeatherSettings
{
    public const string SectionName = "Weather";

    public string Provider { get; set; } = "OpenWeather";

    /// <summary>Base API host, e.g. https://api.openweathermap.org</summary>
    public string BaseUrl { get; set; } = "https://api.openweathermap.org";

    /// <summary>Stored via user-secrets / environment. Never commit real keys.</summary>
    public string ApiKey { get; set; } = "08b191c6e1321c7c88ba499c298aa6e5";

    public int CacheMinutes { get; set; } = 30;

    public int TimeoutSeconds { get; set; } = 15;
}
