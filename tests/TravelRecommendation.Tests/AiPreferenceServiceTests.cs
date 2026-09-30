using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.DTOs.Ai;
using TravelRecommendation.Application.Interfaces;
using TravelRecommendation.Domain.Entities;
using TravelRecommendation.Infrastructure.ExternalServices.OpenAI;
using TravelRecommendation.Infrastructure.Persistence;
using TravelRecommendation.Infrastructure.Services;

namespace TravelRecommendation.Tests;

public class AiPreferenceServiceTests
{
    [Fact]
    public async Task ExtractPreferences_MapsInterestNamesToIds()
    {
        await using var db = CreateDbContext(nameof(ExtractPreferences_MapsInterestNamesToIds));
        SeedInterests(db);

        var openAi = new Mock<IOpenAiTextClient>();
        openAi
            .Setup(x => x.CompleteJsonAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("""
                {
                  "budget": 1200,
                  "budgetCurrency": "EUR",
                  "duration": 7,
                  "tripType": "Beach",
                  "travelPeriod": "June",
                  "preferredClimate": null,
                  "minTemperature": null,
                  "maxTemperature": null,
                  "interests": ["Beach", "Food", "History"],
                  "unmappedPreferences": [],
                  "additionalRequirements": null
                }
                """);

        var service = new OpenAiPreferenceService(openAi.Object, db, new UserPreferenceService(db));
        var result = await service.ExtractPreferencesAsync(new AiPreferenceRequestDto
        {
            Message = "Želim sedam dana na moru u junu. Budžet mi je 1200 eura. Volim hranu i istoriju."
        });

        Assert.Equal(1200, result.Budget);
        Assert.Equal("EUR", result.BudgetCurrency);
        Assert.Equal(7, result.Duration);
        Assert.Equal("Beach", result.TripType);
        Assert.Equal("June", result.TravelPeriod);
        Assert.Equal(3, result.Interests.Count);
        Assert.All(result.Interests, i => Assert.True(i.Id > 0));
        Assert.Contains(result.Interests, i => i.Name == "Beach");
        Assert.Contains(result.Interests, i => i.Name == "Food");
        Assert.Contains(result.Interests, i => i.Name == "History");
        Assert.False(result.RequiresCurrencyConversion);
    }

    [Fact]
    public async Task ExtractPreferences_UnsupportedInterestGoesToUnmapped()
    {
        await using var db = CreateDbContext(nameof(ExtractPreferences_UnsupportedInterestGoesToUnmapped));
        SeedInterests(db);

        var openAi = new Mock<IOpenAiTextClient>();
        openAi
            .Setup(x => x.CompleteJsonAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("""
                {
                  "budget": null,
                  "budgetCurrency": null,
                  "duration": null,
                  "tripType": null,
                  "travelPeriod": null,
                  "preferredClimate": null,
                  "minTemperature": null,
                  "maxTemperature": null,
                  "interests": ["Nature", "Photography"],
                  "unmappedPreferences": [],
                  "additionalRequirements": null
                }
                """);

        var service = new OpenAiPreferenceService(openAi.Object, db, new UserPreferenceService(db));
        var result = await service.ExtractPreferencesAsync(new AiPreferenceRequestDto
        {
            Message = "Želim putovanje gdje mogu puno fotografisati i volim prirodu."
        });

        Assert.Single(result.Interests);
        Assert.Equal("Nature", result.Interests[0].Name);
        Assert.Contains("Photography", result.UnmappedPreferences);
    }

    [Fact]
    public async Task ExtractPreferences_BamCurrencyRequiresConversionFlag()
    {
        await using var db = CreateDbContext(nameof(ExtractPreferences_BamCurrencyRequiresConversionFlag));
        SeedInterests(db);

        var openAi = new Mock<IOpenAiTextClient>();
        openAi
            .Setup(x => x.CompleteJsonAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("""
                {
                  "budget": 1500,
                  "budgetCurrency": "BAM",
                  "duration": 5,
                  "tripType": "City Break",
                  "travelPeriod": null,
                  "preferredClimate": null,
                  "minTemperature": null,
                  "maxTemperature": null,
                  "interests": ["Culture", "History", "Food"],
                  "unmappedPreferences": [],
                  "additionalRequirements": null
                }
                """);

        var service = new OpenAiPreferenceService(openAi.Object, db, new UserPreferenceService(db));
        var result = await service.ExtractPreferencesAsync(new AiPreferenceRequestDto
        {
            Message = "Imam 1500 KM i želim pet dana u nekom evropskom gradu. Zanimaju me muzeji, istorija i lokalna hrana."
        });

        Assert.Equal(1500, result.Budget);
        Assert.Equal("BAM", result.BudgetCurrency);
        Assert.True(result.RequiresCurrencyConversion);
        Assert.NotNull(result.CurrencyNote);
    }

    [Fact]
    public async Task CreatePreference_FromConfirmedEurData_ReturnsId()
    {
        await using var db = CreateDbContext(nameof(CreatePreference_FromConfirmedEurData_ReturnsId));
        SeedInterests(db);
        db.Users.Add(new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "ai-pref@test.local",
            PasswordHash = "x",
            CreatedAt = DateTime.UtcNow,
            RoleId = 1,
            Role = new Role { Name = "User" }
        });
        await db.SaveChangesAsync();
        var userId = db.Users.Single().Id;
        var beachId = db.Interests.Single(i => i.Name == "Beach").Id;

        var openAi = new Mock<IOpenAiTextClient>(MockBehavior.Strict);
        var service = new OpenAiPreferenceService(openAi.Object, db, new UserPreferenceService(db));

        var result = await service.CreatePreferenceAsync(userId, new CreatePreferenceFromAiRequestDto
        {
            Budget = 1200,
            BudgetCurrency = "EUR",
            Duration = 7,
            TripType = "Beach",
            TravelPeriod = "June",
            InterestIds = [beachId]
        });

        Assert.True(result.UserPreferenceId > 0);
        openAi.VerifyNoOtherCalls();
    }

    private static TravelDbContext CreateDbContext(string name)
    {
        var options = new DbContextOptionsBuilder<TravelDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new TravelDbContext(options);
    }

    private static void SeedInterests(TravelDbContext db)
    {
        foreach (var name in Application.Ai.SupportedTravelTaxonomy.Interests)
        {
            db.Interests.Add(new Interest { Name = name });
        }

        db.SaveChanges();
    }
}

public class AiRecommendationExplanationServiceTests
{
    [Fact]
    public async Task GenerateExplanations_OnOpenAiFailure_KeepsScoreRankAndDeterministicExplanation()
    {
        await using var db = CreateDbContext(nameof(GenerateExplanations_OnOpenAiFailure_KeepsScoreRankAndDeterministicExplanation));

        var nature = new Interest { Name = "Nature" };
        var destination = new Destination
        {
            Name = "Interlaken",
            Country = "Switzerland",
            City = "Interlaken",
            AverageDailyCost = 150,
            Latitude = 1,
            Longitude = 1,
            Popularity = 0.8m,
            CreatedAt = DateTime.UtcNow,
            DestinationInterests = [new DestinationInterest { Interest = nature }]
        };

        var user = new User
        {
            FirstName = "A",
            LastName = "B",
            Email = "ai-expl@test.local",
            PasswordHash = "x",
            CreatedAt = DateTime.UtcNow,
            Role = new Role { Name = "User" }
        };

        var preference = new UserPreference
        {
            User = user,
            Budget = 2000,
            Duration = 10,
            TripType = "Relaxation",
            PreferredClimate = "Mild",
            CreatedAt = DateTime.UtcNow,
            UserPreferenceInterests = [new UserPreferenceInterest { Interest = nature }]
        };

        var recommendation = new Recommendation
        {
            User = user,
            UserPreference = preference,
            CreatedAt = DateTime.UtcNow,
            RecommendationItems =
            [
                new RecommendationItem
                {
                    Destination = destination,
                    Score = 0.8735m,
                    Rank = 1,
                    EstimatedCost = 1050,
                    Explanation = "Deterministic match for Nature and budget fit."
                }
            ]
        };

        db.Recommendations.Add(recommendation);
        await db.SaveChangesAsync();

        var openAi = new Mock<IOpenAiTextClient>();
        openAi
            .Setup(x => x.CompleteTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ServiceUnavailableAppException("OpenAI down"));

        var service = new OpenAiRecommendationExplanationService(
            db,
            openAi.Object,
            NullLogger<OpenAiRecommendationExplanationService>.Instance);

        var result = await service.GenerateExplanationsAsync(user.Id, recommendation.Id);

        Assert.True(result.UsedFallback);
        Assert.Single(result.Items);
        Assert.Equal(0.8735m, result.Items[0].Score);
        Assert.Equal(1, result.Items[0].Rank);
        Assert.Equal(1050, result.Items[0].EstimatedCost);
        Assert.Equal("Deterministic match for Nature and budget fit.", result.Items[0].DeterministicExplanation);
        Assert.Null(result.Items[0].AiExplanation);

        var stored = await db.RecommendationItems.SingleAsync();
        Assert.Equal(0.8735m, stored.Score);
        Assert.Equal(1, stored.Rank);
        Assert.Equal(1050, stored.EstimatedCost);
        Assert.Equal("Deterministic match for Nature and budget fit.", stored.Explanation);
        Assert.Null(stored.AiExplanation);
    }

    [Fact]
    public async Task GenerateExplanations_DoesNotChangeScoreWhenAiSucceeds()
    {
        await using var db = CreateDbContext(nameof(GenerateExplanations_DoesNotChangeScoreWhenAiSucceeds));

        var beach = new Interest { Name = "Beach" };
        var destination = new Destination
        {
            Name = "Barcelona",
            Country = "Spain",
            City = "Barcelona",
            AverageDailyCost = 120,
            Latitude = 1,
            Longitude = 1,
            Popularity = 0.9m,
            CreatedAt = DateTime.UtcNow,
            DestinationInterests = [new DestinationInterest { Interest = beach }]
        };

        var user = new User
        {
            FirstName = "A",
            LastName = "B",
            Email = "ai-expl2@test.local",
            PasswordHash = "x",
            CreatedAt = DateTime.UtcNow,
            Role = new Role { Name = "User" }
        };

        var preference = new UserPreference
        {
            User = user,
            Budget = 1200,
            Duration = 7,
            TripType = "Beach",
            CreatedAt = DateTime.UtcNow,
            UserPreferenceInterests = [new UserPreferenceInterest { Interest = beach }]
        };

        var recommendation = new Recommendation
        {
            User = user,
            UserPreference = preference,
            CreatedAt = DateTime.UtcNow,
            RecommendationItems =
            [
                new RecommendationItem
                {
                    Destination = destination,
                    Score = 0.91m,
                    Rank = 1,
                    EstimatedCost = 980,
                    Explanation = "Strong beach and budget match."
                }
            ]
        };

        db.Recommendations.Add(recommendation);
        await db.SaveChangesAsync();

        var openAi = new Mock<IOpenAiTextClient>();
        openAi
            .Setup(x => x.CompleteTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Barcelona se dobro uklapa zbog plaže i budžeta.");

        var service = new OpenAiRecommendationExplanationService(
            db,
            openAi.Object,
            NullLogger<OpenAiRecommendationExplanationService>.Instance);

        var result = await service.GenerateExplanationsAsync(user.Id, recommendation.Id);

        Assert.False(result.UsedFallback);
        Assert.Equal(0.91m, result.Items[0].Score);
        Assert.Equal(1, result.Items[0].Rank);
        Assert.Equal(980, result.Items[0].EstimatedCost);
        Assert.Equal("Barcelona se dobro uklapa zbog plaže i budžeta.", result.Items[0].AiExplanation);

        var stored = await db.RecommendationItems.SingleAsync();
        Assert.Equal(0.91m, stored.Score);
        Assert.Equal("Strong beach and budget match.", stored.Explanation);
        Assert.Equal("Barcelona se dobro uklapa zbog plaže i budžeta.", stored.AiExplanation);
    }

    private static TravelDbContext CreateDbContext(string name)
    {
        var options = new DbContextOptionsBuilder<TravelDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new TravelDbContext(options);
    }
}
