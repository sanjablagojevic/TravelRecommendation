using TravelRecommendation.Application.Recommendations;

namespace TravelRecommendation.Tests;

public class RecommendationScoringServiceTests
{
    private readonly RecommendationScoringService _sut;

    public RecommendationScoringServiceTests()
    {
        var weights = new RecommendationWeights();
        weights.EnsureValid();
        _sut = new RecommendationScoringService(weights);
    }

    [Fact]
    public void InterestScore_MatchesThreeOfFour_Returns075()
    {
        var preference = new PreferenceScoringInput
        {
            InterestNames = ["Beach", "Culture", "Food", "Nature"]
        };

        var destination = new DestinationScoringInput
        {
            Id = 1,
            Name = "Barcelona",
            AverageDailyCost = 100m,
            Popularity = 0.9m,
            InterestNames = ["Beach", "Culture", "Food", "History"]
        };

        var result = _sut.ScoreDestination(preference, destination);

        Assert.Equal(0.75m, result.Breakdown.InterestScore);
        Assert.Equal(3, result.MatchedInterests.Count);
        Assert.Contains("Nature", result.MissingInterests);
    }

    [Fact]
    public void BudgetScore_WithinBudget_Returns1()
    {
        // Budget=1000, Duration=5, AverageDailyCost=150 => EstimatedCost=750
        var score = RecommendationScoringService.CalculateBudgetScore(1000m, 750m);
        Assert.Equal(1.0m, score);
    }

    [Fact]
    public void BudgetScore_OverBudget_Returns08()
    {
        // Budget=1000, Duration=5, AverageDailyCost=250 => EstimatedCost=1250
        var score = RecommendationScoringService.CalculateBudgetScore(1000m, 1250m);
        Assert.Equal(0.8m, score);
    }

    [Fact]
    public void BudgetAndEstimatedCost_AreCalculatedFromPreference()
    {
        var preference = new PreferenceScoringInput
        {
            Budget = 1000m,
            Duration = 5
        };

        var destination = new DestinationScoringInput
        {
            Id = 1,
            Name = "Test",
            AverageDailyCost = 250m,
            Popularity = 0.5m
        };

        var result = _sut.ScoreDestination(preference, destination);

        Assert.Equal(1250m, result.EstimatedCost);
        Assert.Equal(0.8m, result.Breakdown.BudgetScore);
    }

    [Fact]
    public void TripType_Match_Returns1()
    {
        var preference = new PreferenceScoringInput { TripType = "Beach" };
        var destination = new DestinationScoringInput
        {
            Id = 1,
            Name = "Malaga",
            AverageDailyCost = 80m,
            Popularity = 0.7m,
            InterestNames = ["Beach", "Food"]
        };

        var result = _sut.ScoreDestination(preference, destination);
        Assert.Equal(1.0m, result.Breakdown.TripTypeScore);
    }

    [Fact]
    public void TripType_Mismatch_Returns0()
    {
        var preference = new PreferenceScoringInput { TripType = "Mountains" };
        var destination = new DestinationScoringInput
        {
            Id = 1,
            Name = "Malaga",
            AverageDailyCost = 80m,
            Popularity = 0.7m,
            InterestNames = ["Beach", "Food"],
            CategoryNames = ["City Break"]
        };

        var result = _sut.ScoreDestination(preference, destination);
        Assert.Equal(0.0m, result.Breakdown.TripTypeScore);
    }

    [Fact]
    public void Climate_Match_Returns1()
    {
        var preference = new PreferenceScoringInput { PreferredClimate = "Mediterranean" };
        var destination = new DestinationScoringInput
        {
            Id = 1,
            Name = "Rome",
            AverageDailyCost = 100m,
            Popularity = 0.8m,
            Climate = "mediterranean"
        };

        var result = _sut.ScoreDestination(preference, destination);
        Assert.Equal(1.0m, result.Breakdown.ClimateScore);
    }

    [Fact]
    public void Climate_Mismatch_Returns0()
    {
        var preference = new PreferenceScoringInput { PreferredClimate = "Mediterranean" };
        var destination = new DestinationScoringInput
        {
            Id = 1,
            Name = "Vienna",
            AverageDailyCost = 100m,
            Popularity = 0.8m,
            Climate = "Continental"
        };

        var result = _sut.ScoreDestination(preference, destination);
        Assert.Equal(0.0m, result.Breakdown.ClimateScore);
    }

    [Fact]
    public void Popularity_IsClampedAndUsed()
    {
        var preference = new PreferenceScoringInput();
        var destination = new DestinationScoringInput
        {
            Id = 1,
            Name = "Paris",
            AverageDailyCost = 120m,
            Popularity = 0.97m
        };

        var result = _sut.ScoreDestination(preference, destination);
        Assert.Equal(0.97m, result.Breakdown.PopularityScore);
        Assert.Equal(0.97m, result.FinalScore);
    }

    [Fact]
    public void DynamicWeightNormalization_UsesExpectedFormula()
    {
        // Interest 0.30 * 0.75 + Budget 0.25 * 1.0 + Popularity 0.15 * 0.80
        // / (0.30 + 0.25 + 0.15) = 0.545 / 0.70 = 0.778571... => 0.7786
        var preference = new PreferenceScoringInput
        {
            Budget = 1000m,
            Duration = 5,
            InterestNames = ["Beach", "Culture", "Food", "Nature"]
        };

        var destination = new DestinationScoringInput
        {
            Id = 1,
            Name = "Barcelona",
            AverageDailyCost = 150m, // EstimatedCost = 750 <= 1000 => BudgetScore 1.0
            Popularity = 0.80m,
            InterestNames = ["Beach", "Culture", "Food", "History"]
        };

        var result = _sut.ScoreDestination(preference, destination);

        var expected =
            (0.30m * 0.75m + 0.25m * 1.0m + 0.15m * 0.80m)
            / (0.30m + 0.25m + 0.15m);
        expected = Math.Round(expected, 4, MidpointRounding.AwayFromZero);

        Assert.Equal(0.75m, result.Breakdown.InterestScore);
        Assert.Equal(1.0m, result.Breakdown.BudgetScore);
        Assert.Equal(0.80m, result.Breakdown.PopularityScore);
        Assert.Null(result.Breakdown.TripTypeScore);
        Assert.Null(result.Breakdown.ClimateScore);
        Assert.Equal(expected, result.FinalScore);
        Assert.True(result.FinalScore is >= 0m and <= 1m);
    }

    [Fact]
    public void MissingOptionalPreferences_DoNotActivateThoseCriteria()
    {
        var preference = new PreferenceScoringInput
        {
            InterestNames = ["Culture"]
        };

        var destination = new DestinationScoringInput
        {
            Id = 1,
            Name = "Rome",
            AverageDailyCost = 110m,
            Popularity = 0.9m,
            InterestNames = ["Culture", "History"]
        };

        var result = _sut.ScoreDestination(preference, destination);

        Assert.Null(result.Breakdown.BudgetScore);
        Assert.Null(result.Breakdown.TripTypeScore);
        Assert.Null(result.Breakdown.ClimateScore);
        Assert.Null(result.EstimatedCost);
        Assert.True(result.Breakdown.Components.Single(c => c.Name == "Budget").IsActive == false);
    }

    [Fact]
    public void Score_AlwaysStaysBetweenZeroAndOne()
    {
        var preference = new PreferenceScoringInput
        {
            Budget = 100m,
            Duration = 10,
            TripType = "Beach",
            PreferredClimate = "Desert",
            InterestNames = ["Beach", "Adventure"]
        };

        var destination = new DestinationScoringInput
        {
            Id = 1,
            Name = "Cairo",
            AverageDailyCost = 200m,
            Popularity = 1.5m, // should clamp
            Climate = "Desert",
            InterestNames = ["Culture"]
        };

        var result = _sut.ScoreDestination(preference, destination);
        Assert.InRange(result.FinalScore, 0m, 1m);
        Assert.Equal(1.0m, result.Breakdown.PopularityScore);
    }

    [Fact]
    public void RankDestinations_OrdersByScoreDescending()
    {
        var preference = new PreferenceScoringInput
        {
            InterestNames = ["Culture"]
        };

        var destinations = new List<DestinationScoringInput>
        {
            new()
            {
                Id = 3,
                Name = "C",
                AverageDailyCost = 50m,
                Popularity = 0.5m,
                InterestNames = []
            },
            new()
            {
                Id = 1,
                Name = "A",
                AverageDailyCost = 50m,
                Popularity = 0.5m,
                InterestNames = ["Culture"]
            },
            new()
            {
                Id = 2,
                Name = "B",
                AverageDailyCost = 50m,
                Popularity = 0.9m,
                InterestNames = []
            }
        };

        // A matches interest => higher score than B/C which rely more on popularity after renormalization
        var ranked = _sut.RankDestinations(preference, destinations, topCount: 3);

        Assert.Equal(3, ranked.Count);
        Assert.Equal(1, ranked[0].DestinationId);
        Assert.True(ranked[0].FinalScore >= ranked[1].FinalScore);
        Assert.True(ranked[1].FinalScore >= ranked[2].FinalScore);
    }

    [Fact]
    public void RankDestinations_TieBreaker_UsesPopularityThenId()
    {
        var preference = new PreferenceScoringInput();

        var destinations = new List<DestinationScoringInput>
        {
            new() { Id = 20, Name = "LowPop", AverageDailyCost = 50m, Popularity = 0.5m },
            new() { Id = 10, Name = "HighPop", AverageDailyCost = 50m, Popularity = 0.9m },
            new() { Id = 11, Name = "SamePopHigherId", AverageDailyCost = 50m, Popularity = 0.9m }
        };

        var ranked = _sut.RankDestinations(preference, destinations, topCount: 3);

        // Same score from popularity-only: higher popularity first, then lower Id
        Assert.Equal(10, ranked[0].DestinationId);
        Assert.Equal(11, ranked[1].DestinationId);
        Assert.Equal(20, ranked[2].DestinationId);
    }

    [Fact]
    public void Weights_MustSumToOne()
    {
        var invalid = new RecommendationWeights
        {
            Interest = 0.5m,
            Budget = 0.5m,
            TripType = 0.5m,
            Climate = 0.5m,
            Popularity = 0.5m
        };

        Assert.Throws<InvalidOperationException>(() => invalid.EnsureValid());
    }
}
