using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.Settings;
using TravelRecommendation.Domain.Entities;
using TravelRecommendation.Infrastructure.ExternalServices.Weather.OpenWeather;
using TravelRecommendation.Infrastructure.Persistence;

namespace TravelRecommendation.Tests;

public class OpenWeatherServiceCacheTests
{
    [Fact]
    public async Task GetDestinationWeather_UsesCacheWhenNotExpired()
    {
        await using var db = CreateDb(nameof(GetDestinationWeather_UsesCacheWhenNotExpired));
        var destination = SeedDestination(db, 41.3874m, 2.1686m);

        var cachedPayload = """
            {
              "destinationId": 1,
              "destinationName": "Barcelona",
              "current": {
                "temperature": 24.0,
                "feelsLike": 24.0,
                "minTemperature": 20.0,
                "maxTemperature": 26.0,
                "humidity": 50,
                "description": "Clear sky",
                "icon": "https://openweathermap.org/img/wn/01d@2x.png",
                "windSpeed": 10.0,
                "observedAt": "2026-06-01T12:00:00Z"
              },
              "forecast": [],
              "fetchedAt": "2026-06-01T12:00:00Z",
              "isCached": false,
              "isStale": false
            }
            """;

        db.ApiCaches.Add(new ApiCache
        {
            Provider = "OpenWeather",
            Action = OpenWeatherService.CacheAction,
            RequestKey = OpenWeatherService.BuildCacheKey(destination.Id),
            ResponseJson = cachedPayload.Replace("\"destinationId\": 1", $"\"destinationId\": {destination.Id}"),
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            ExpiresAt = DateTime.UtcNow.AddMinutes(25)
        });
        await db.SaveChangesAsync();

        var httpFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        var service = CreateService(db, httpFactory.Object, apiKey: "unused");

        var result = await service.GetDestinationWeatherAsync(destination.Id);

        Assert.True(result.IsCached);
        Assert.False(result.IsStale);
        Assert.Equal(24.0m, result.Current.Temperature);
        httpFactory.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetDestinationWeather_ThrowsNotFound_WithoutCallingProvider()
    {
        await using var db = CreateDb(nameof(GetDestinationWeather_ThrowsNotFound_WithoutCallingProvider));
        var httpFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        var service = CreateService(db, httpFactory.Object, apiKey: "key");

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetDestinationWeatherAsync(999999));

        httpFactory.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetDestinationWeather_RejectsInvalidCoordinates()
    {
        await using var db = CreateDb(nameof(GetDestinationWeather_RejectsInvalidCoordinates));
        var destination = SeedDestination(db, 0m, 0m);
        var httpFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        var service = CreateService(db, httpFactory.Object, apiKey: "key");

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.GetDestinationWeatherAsync(destination.Id));

        httpFactory.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetDestinationWeather_WithoutApiKey_ThrowsServiceUnavailable()
    {
        await using var db = CreateDb(nameof(GetDestinationWeather_WithoutApiKey_ThrowsServiceUnavailable));
        var destination = SeedDestination(db, 41.3874m, 2.1686m);
        var httpFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        var service = CreateService(db, httpFactory.Object, apiKey: "");

        await Assert.ThrowsAsync<ServiceUnavailableAppException>(() =>
            service.GetDestinationWeatherAsync(destination.Id));

        httpFactory.VerifyNoOtherCalls();
    }

    private static OpenWeatherService CreateService(
        TravelDbContext db,
        IHttpClientFactory httpFactory,
        string apiKey)
    {
        var settings = Options.Create(new WeatherSettings
        {
            Provider = "OpenWeather",
            BaseUrl = "https://api.openweathermap.org",
            ApiKey = apiKey,
            CacheMinutes = 30,
            TimeoutSeconds = 15
        });

        return new OpenWeatherService(
            httpFactory,
            db,
            settings,
            NullLogger<OpenWeatherService>.Instance);
    }

    private static TravelDbContext CreateDb(string name)
    {
        var options = new DbContextOptionsBuilder<TravelDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new TravelDbContext(options);
    }

    private static Destination SeedDestination(TravelDbContext db, decimal lat, decimal lon)
    {
        var destination = new Destination
        {
            Name = "Barcelona",
            Country = "Spain",
            City = "Barcelona",
            AverageDailyCost = 95,
            Latitude = lat,
            Longitude = lon,
            Popularity = 0.9m,
            Climate = "Mediterranean",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Destinations.Add(destination);
        db.SaveChanges();
        return destination;
    }
}
