using System.Text.Json.Serialization;

namespace TravelRecommendation.Infrastructure.ExternalServices.Weather.OpenWeather;

internal sealed class OpenWeatherCurrentResponse
{
    [JsonPropertyName("dt")]
    public long Dt { get; set; }

    [JsonPropertyName("main")]
    public OpenWeatherMain? Main { get; set; }

    [JsonPropertyName("weather")]
    public List<OpenWeatherCondition> Weather { get; set; } = [];

    [JsonPropertyName("wind")]
    public OpenWeatherWind? Wind { get; set; }
}

internal sealed class OpenWeatherForecastResponse
{
    [JsonPropertyName("list")]
    public List<OpenWeatherForecastItem> List { get; set; } = [];
}

internal sealed class OpenWeatherForecastItem
{
    [JsonPropertyName("dt")]
    public long Dt { get; set; }

    [JsonPropertyName("main")]
    public OpenWeatherMain? Main { get; set; }

    [JsonPropertyName("weather")]
    public List<OpenWeatherCondition> Weather { get; set; } = [];
}

internal sealed class OpenWeatherMain
{
    [JsonPropertyName("temp")]
    public decimal Temp { get; set; }

    [JsonPropertyName("feels_like")]
    public decimal FeelsLike { get; set; }

    [JsonPropertyName("temp_min")]
    public decimal TempMin { get; set; }

    [JsonPropertyName("temp_max")]
    public decimal TempMax { get; set; }

    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
}

internal sealed class OpenWeatherCondition
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;
}

internal sealed class OpenWeatherWind
{
    [JsonPropertyName("speed")]
    public decimal Speed { get; set; }
}
