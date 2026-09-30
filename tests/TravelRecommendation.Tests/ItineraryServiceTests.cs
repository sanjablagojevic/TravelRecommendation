using Moq;
using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.DTOs.Itineraries;
using TravelRecommendation.Application.Interfaces;
using TravelRecommendation.Application.Itineraries;
using TravelRecommendation.Application.Settings;
using TravelRecommendation.Domain.Entities;
using TravelRecommendation.Infrastructure.Persistence;
using TravelRecommendation.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace TravelRecommendation.Tests;

public class ItineraryServiceTests
{
    [Fact]
    public void ValidateAndNormalize_RejectsWrongDayCount()
    {
        var raw = BuildValidRaw(days: 3);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            AiItineraryOutputValidator.ValidateAndNormalize(raw, expectedDays: 4, new HashSet<int>(), new HashSet<int>()));
        Assert.Contains("exactly 4 days", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateAndNormalize_RejectsForeignAttractionId()
    {
        var raw = BuildValidRaw(days: 1);
        raw.Days[0].Items[0].ItemType = "Attraction";
        raw.Days[0].Items[0].AttractionId = 999;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            AiItineraryOutputValidator.ValidateAndNormalize(raw, 1, new HashSet<int> { 1 }, new HashSet<int>()));
        Assert.Contains("attractionId", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateAndNormalize_RejectsForeignActivityId()
    {
        var raw = BuildValidRaw(days: 1);
        raw.Days[0].Items[0].ItemType = "Activity";
        raw.Days[0].Items[0].ActivityId = 999;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            AiItineraryOutputValidator.ValidateAndNormalize(raw, 1, new HashSet<int>(), new HashSet<int> { 1 }));
        Assert.Contains("activityId", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateAndNormalize_AcceptsValidFourDays()
    {
        var raw = BuildValidRaw(days: 4);
        var result = AiItineraryOutputValidator.ValidateAndNormalize(
            raw, 4, new HashSet<int> { 10 }, new HashSet<int> { 20 });

        Assert.Equal(4, result.Days.Count);
        Assert.Equal(new[] { 1, 2, 3, 4 }, result.Days.Select(d => d.DayNumber));
    }

    [Fact]
    public void BuildItineraryInstructions_ContainHallucinationGuards()
    {
        var instructions = Application.Ai.OpenAiJsonSchemas.BuildItineraryGenerationInstructions();
        Assert.Contains("Do not invent factual details", instructions);
        Assert.Contains("ticket prices", instructions);
        Assert.Contains("opening hours", instructions);
        Assert.Contains("Do not recommend a different destination", instructions);
        Assert.Contains("real-time weather", instructions);
    }

    [Fact]
    public async Task Generate_PersistsValidatedItinerary()
    {
        await using var db = CreateDb(nameof(Generate_PersistsValidatedItinerary));
        var destination = SeedDestination(db);
        var ai = new Mock<IAiItineraryService>();
        ai.Setup(x => x.GenerateAsync(It.IsAny<AiItineraryGenerationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildValidRaw(days: 2, attractionId: destination.Attractions.First().Id));

        var service = CreateService(db, ai.Object);
        var result = await service.GenerateAsync(1, new GenerateItineraryRequestDto
        {
            DestinationId = destination.Id,
            DurationDays = 2
        });

        Assert.Equal(2, result.Days.Count);
        Assert.Equal(2, await db.TravelItineraryDays.CountAsync());
        Assert.True(await db.TravelItineraryItems.AnyAsync());
        ai.Verify(x => x.GenerateAsync(
            It.Is<AiItineraryGenerationContext>(c => c.DestinationId == destination.Id && c.DurationDays == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetById_Returns404_ForOtherUser()
    {
        await using var db = CreateDb(nameof(GetById_Returns404_ForOtherUser));
        var destination = SeedDestination(db);
        db.TravelItineraries.Add(new TravelItinerary
        {
            UserId = 1,
            DestinationId = destination.Id,
            Title = "Private plan",
            DurationDays = 1,
            CreatedAt = DateTime.UtcNow,
            Days =
            [
                new TravelItineraryDay
                {
                    DayNumber = 1,
                    Title = "Day 1",
                    Items =
                    [
                        new TravelItineraryItem
                        {
                            Order = 1,
                            TimeOfDay = "Morning",
                            Title = "Walk",
                            ItemType = Domain.Enums.ItineraryItemType.General
                        }
                    ]
                }
            ]
        });
        await db.SaveChangesAsync();
        var id = db.TravelItineraries.Single().Id;

        var service = CreateService(db, Mock.Of<IAiItineraryService>());
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(2, id));
    }

    [Fact]
    public async Task Generate_PropagatesProviderFailure_WithoutPersisting()
    {
        await using var db = CreateDb(nameof(Generate_PropagatesProviderFailure_WithoutPersisting));
        var destination = SeedDestination(db);
        var ai = new Mock<IAiItineraryService>();
        ai.Setup(x => x.GenerateAsync(It.IsAny<AiItineraryGenerationContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ServiceUnavailableAppException("Itinerary generation is temporarily unavailable."));

        var service = CreateService(db, ai.Object);
        await Assert.ThrowsAsync<ServiceUnavailableAppException>(() =>
            service.GenerateAsync(1, new GenerateItineraryRequestDto
            {
                DestinationId = destination.Id,
                DurationDays = 2
            }));

        Assert.Empty(db.TravelItineraries);
    }

    private static ItineraryService CreateService(TravelDbContext db, IAiItineraryService ai) =>
        new(
            db,
            ai,
            Options.Create(new OpenAiSettings { Model = "gpt-4o-mini" }),
            NullLogger<ItineraryService>.Instance);

    private static TravelDbContext CreateDb(string name)
    {
        var options = new DbContextOptionsBuilder<TravelDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        var db = new TravelDbContext(options);
        db.Users.Add(new User
        {
            Id = 1,
            FirstName = "A",
            LastName = "User",
            Email = $"{name}@test.com",
            PasswordHash = "x",
            CreatedAt = DateTime.UtcNow,
            RoleId = 1
        });
        db.Roles.Add(new Role { Id = 1, Name = "User" });
        db.SaveChanges();
        return db;
    }

    private static Destination SeedDestination(TravelDbContext db)
    {
        var destination = new Destination
        {
            Name = "Barcelona",
            Country = "Spain",
            City = "Barcelona",
            AverageDailyCost = 100,
            Latitude = 41.3m,
            Longitude = 2.1m,
            Popularity = 0.9m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Attractions =
            [
                new Attraction { Name = "Sagrada Familia", Location = "Barcelona" }
            ],
            Activities =
            [
                new Activity { Name = "Walking Tour", Provider = "Local", Price = 35, Currency = "EUR" }
            ]
        };
        db.Destinations.Add(destination);
        db.SaveChanges();
        return destination;
    }

    private static AiGeneratedItineraryDto BuildValidRaw(int days, int? attractionId = null)
    {
        var result = new AiGeneratedItineraryDto
        {
            Title = $"{days} Days in Barcelona",
            Days = []
        };

        for (var d = 1; d <= days; d++)
        {
            result.Days.Add(new AiGeneratedItineraryDayDto
            {
                DayNumber = d,
                Title = $"Day {d}",
                Summary = "Summary",
                Items =
                [
                    new AiGeneratedItineraryItemDto
                    {
                        Order = 1,
                        TimeOfDay = "Morning",
                        Title = attractionId.HasValue && d == 1 ? "Sagrada Familia" : "Explore old town",
                        Description = "Enjoy the area",
                        ItemType = attractionId.HasValue && d == 1 ? "Attraction" : "General",
                        AttractionId = attractionId.HasValue && d == 1 ? attractionId : null,
                        ActivityId = null,
                        Location = "Barcelona"
                    },
                    new AiGeneratedItineraryItemDto
                    {
                        Order = 2,
                        TimeOfDay = "Afternoon",
                        Title = "Local food",
                        Description = "Try local cuisine",
                        ItemType = "General",
                        AttractionId = null,
                        ActivityId = null,
                        Location = null
                    }
                ]
            });
        }

        return result;
    }
}
