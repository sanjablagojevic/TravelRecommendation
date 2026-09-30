using Microsoft.EntityFrameworkCore;
using TravelRecommendation.Application.DTOs.Destinations;
using TravelRecommendation.Domain.Entities;
using TravelRecommendation.Infrastructure.ExternalServices.Weather.OpenWeather;
using TravelRecommendation.Infrastructure.Persistence;
using TravelRecommendation.Infrastructure.Services;

namespace TravelRecommendation.Tests;

public class DestinationWeatherCacheInvalidationTests
{
    [Fact]
    public async Task UpdateDestination_InvalidatesWeatherCache_WhenCoordinatesChange()
    {
        await using var db = CreateDb(nameof(UpdateDestination_InvalidatesWeatherCache_WhenCoordinatesChange));
        var destination = new Destination
        {
            Name = "Barcelona",
            Country = "Spain",
            City = "Barcelona",
            AverageDailyCost = 100,
            Latitude = 41.3874m,
            Longitude = 2.1686m,
            Popularity = 0.9m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Destinations.Add(destination);
        await db.SaveChangesAsync();

        db.ApiCaches.Add(new ApiCache
        {
            Provider = "OpenWeather",
            Action = OpenWeatherService.CacheAction,
            RequestKey = OpenWeatherService.BuildCacheKey(destination.Id),
            ResponseJson = "{\"destinationId\":1}",
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            ExpiresAt = DateTime.UtcNow.AddMinutes(20)
        });
        await db.SaveChangesAsync();

        var service = new DestinationService(db);
        await service.UpdateAsync(destination.Id, new UpdateDestinationRequestDto
        {
            Name = destination.Name,
            Country = destination.Country,
            City = destination.City,
            AverageDailyCost = destination.AverageDailyCost,
            Latitude = 41.4m,
            Longitude = 2.2m,
            Popularity = destination.Popularity,
            IsActive = true,
            CategoryIds = [],
            InterestIds = []
        });

        Assert.Empty(await db.ApiCaches.ToListAsync());
    }

    [Fact]
    public async Task UpdateDestination_KeepsWeatherCache_WhenCoordinatesUnchanged()
    {
        await using var db = CreateDb(nameof(UpdateDestination_KeepsWeatherCache_WhenCoordinatesUnchanged));
        var destination = new Destination
        {
            Name = "Rome",
            Country = "Italy",
            City = "Rome",
            AverageDailyCost = 120,
            Latitude = 41.9m,
            Longitude = 12.5m,
            Popularity = 0.8m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Destinations.Add(destination);
        await db.SaveChangesAsync();

        db.ApiCaches.Add(new ApiCache
        {
            Provider = "OpenWeather",
            Action = OpenWeatherService.CacheAction,
            RequestKey = OpenWeatherService.BuildCacheKey(destination.Id),
            ResponseJson = "{\"destinationId\":1}",
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            ExpiresAt = DateTime.UtcNow.AddMinutes(20)
        });
        await db.SaveChangesAsync();

        var service = new DestinationService(db);
        await service.UpdateAsync(destination.Id, new UpdateDestinationRequestDto
        {
            Name = "Roma",
            Country = destination.Country,
            City = destination.City,
            AverageDailyCost = 130,
            Latitude = destination.Latitude,
            Longitude = destination.Longitude,
            Popularity = destination.Popularity,
            IsActive = true,
            CategoryIds = [],
            InterestIds = []
        });

        Assert.Single(await db.ApiCaches.ToListAsync());
    }

    private static TravelDbContext CreateDb(string name)
    {
        var options = new DbContextOptionsBuilder<TravelDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new TravelDbContext(options);
    }
}
