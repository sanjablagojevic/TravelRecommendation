using TravelRecommendation.Application.DTOs.Weather;
using TravelRecommendation.Infrastructure.ExternalServices.Weather.OpenWeather;

namespace TravelRecommendation.Tests;

public class OpenWeatherMappingTests
{
    [Fact]
    public void MapProviderResponses_ConvertsWindToKmPerHour_AndBuildsIconUrl()
    {
        var current = new OpenWeatherCurrentResponse
        {
            Dt = 1_700_000_000,
            Main = new OpenWeatherMain
            {
                Temp = 24.32m,
                FeelsLike = 24.8m,
                TempMin = 21.0m,
                TempMax = 26.0m,
                Humidity = 63
            },
            Weather = [new OpenWeatherCondition { Description = "clear sky", Icon = "01d" }],
            Wind = new OpenWeatherWind { Speed = 5m } // m/s -> 18 km/h
        };

        var forecast = new OpenWeatherForecastResponse
        {
            List =
            [
                new OpenWeatherForecastItem
                {
                    Dt = 1_700_003_600,
                    Main = new OpenWeatherMain { Temp = 22m, TempMin = 20m, TempMax = 23m },
                    Weather = [new OpenWeatherCondition { Description = "few clouds", Icon = "02d" }]
                },
                new OpenWeatherForecastItem
                {
                    Dt = 1_700_014_400,
                    Main = new OpenWeatherMain { Temp = 25m, TempMin = 24m, TempMax = 26m },
                    Weather = [new OpenWeatherCondition { Description = "few clouds", Icon = "02d" }]
                }
            ]
        };

        var result = OpenWeatherService.MapProviderResponses(
            1,
            "Barcelona",
            current,
            forecast,
            DateTime.UtcNow);

        Assert.Equal(1, result.DestinationId);
        Assert.Equal("Barcelona", result.DestinationName);
        Assert.Equal(24.3m, result.Current.Temperature);
        Assert.Equal(18.0m, result.Current.WindSpeed);
        Assert.Equal("Clear sky", result.Current.Description);
        Assert.Equal("https://openweathermap.org/img/wn/01d@2x.png", result.Current.Icon);
        Assert.NotEmpty(result.Forecast);
        Assert.True(result.Forecast[0].MinTemperature <= result.Forecast[0].MaxTemperature);
        Assert.Equal(20.0m, result.Forecast[0].MinTemperature);
    }

    [Theory]
    [InlineData(0, 0, false)]
    [InlineData(41.3874, 2.1686, true)]
    [InlineData(91, 10, false)]
    [InlineData(10, -181, false)]
    public void HasValidCoordinates_RejectsSentinelAndOutOfRange(decimal lat, decimal lon, bool expected)
    {
        Assert.Equal(expected, OpenWeatherService.HasValidCoordinates(lat, lon));
    }

    [Fact]
    public void BuildCacheKey_IsStablePerDestination()
    {
        Assert.Equal("weather:destination:7", OpenWeatherService.BuildCacheKey(7));
    }
}

public class OpenWeatherCacheFlowTests
{
    [Fact]
    public void AggregateForecastDays_GroupsByUtcDate_AndLimitsToFive()
    {
        var items = new List<OpenWeatherForecastItem>();
        var start = DateTimeOffset.Parse("2026-06-01T00:00:00Z");
        for (var day = 0; day < 6; day++)
        {
            for (var hour = 0; hour < 24; hour += 3)
            {
                var dt = start.AddDays(day).AddHours(hour);
                items.Add(new OpenWeatherForecastItem
                {
                    Dt = dt.ToUnixTimeSeconds(),
                    Main = new OpenWeatherMain
                    {
                        Temp = 20 + day,
                        TempMin = 18 + day,
                        TempMax = 22 + day
                    },
                    Weather = [new OpenWeatherCondition { Description = "clouds", Icon = "03d" }]
                });
            }
        }

        var days = OpenWeatherService.AggregateForecastDays(items);
        Assert.Equal(5, days.Count);
        Assert.Equal(new DateOnly(2026, 6, 1), days[0].Date);
        Assert.Equal(18.0m, days[0].MinTemperature);
        Assert.Equal(22.0m, days[0].MaxTemperature);
    }
}
