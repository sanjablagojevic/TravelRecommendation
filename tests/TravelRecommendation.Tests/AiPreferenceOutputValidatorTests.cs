using TravelRecommendation.Application.Ai;
using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.DTOs.Ai;

namespace TravelRecommendation.Tests;

public class AiPreferenceOutputValidatorTests
{
    [Fact]
    public void ValidateRequest_RejectsShortMessage()
    {
        var ex = Assert.Throws<ValidationException>(() =>
            AiPreferenceOutputValidator.ValidateRequest(new AiPreferenceRequestDto
            {
                Message = "kratko"
            }));

        Assert.True(ex.Errors.ContainsKey("Message"));
    }

    [Fact]
    public void ValidateAndNormalize_MapsSupportedInterests_AndCollectsUnmapped()
    {
        var result = AiPreferenceOutputValidator.ValidateAndNormalize(new AiPreferenceExtractionDto
        {
            Budget = 1200,
            BudgetCurrency = "EUR",
            Duration = 7,
            TripType = "Beach",
            TravelPeriod = "June",
            Interests = ["Beach", "Food", "Photography"],
            UnmappedPreferences = []
        });

        Assert.Equal(1200, result.Budget);
        Assert.Equal("EUR", result.BudgetCurrency);
        Assert.Equal(7, result.Duration);
        Assert.Equal("Beach", result.TripType);
        Assert.Equal(["Beach", "Food"], result.Interests);
        Assert.Contains("Photography", result.UnmappedPreferences);
    }

    [Fact]
    public void ValidateAndNormalize_NormalizesKmToBam()
    {
        var result = AiPreferenceOutputValidator.ValidateAndNormalize(new AiPreferenceExtractionDto
        {
            Budget = 1500,
            BudgetCurrency = "KM",
            Duration = 5,
            Interests = ["Culture"]
        });

        Assert.Equal("BAM", result.BudgetCurrency);
    }

    [Fact]
    public void ValidateAndNormalize_RejectsInvalidBudget()
    {
        Assert.Throws<ValidationException>(() =>
            AiPreferenceOutputValidator.ValidateAndNormalize(new AiPreferenceExtractionDto
            {
                Budget = 0,
                Interests = []
            }));
    }

    [Fact]
    public void ValidateAndNormalize_RejectsInvalidDuration()
    {
        Assert.Throws<ValidationException>(() =>
            AiPreferenceOutputValidator.ValidateAndNormalize(new AiPreferenceExtractionDto
            {
                Duration = 100,
                Interests = []
            }));
    }

    [Fact]
    public void ValidateAndNormalize_RejectsMinGreaterThanMaxTemperature()
    {
        Assert.Throws<ValidationException>(() =>
            AiPreferenceOutputValidator.ValidateAndNormalize(new AiPreferenceExtractionDto
            {
                MinTemperature = 30,
                MaxTemperature = 20,
                Interests = []
            }));
    }

    [Fact]
    public void ValidateCreatePreference_RejectsNonEurBudget()
    {
        var ex = Assert.Throws<ValidationException>(() =>
            AiPreferenceOutputValidator.ValidateCreatePreference(new CreatePreferenceFromAiRequestDto
            {
                Budget = 1500,
                BudgetCurrency = "BAM",
                Duration = 5,
                InterestIds = [1]
            }));

        Assert.True(ex.Errors.ContainsKey("BudgetCurrency"));
    }

    [Fact]
    public void ValidateCreatePreference_AllowsEurBudget()
    {
        AiPreferenceOutputValidator.ValidateCreatePreference(new CreatePreferenceFromAiRequestDto
        {
            Budget = 1200,
            BudgetCurrency = "EUR",
            Duration = 7,
            TripType = "Beach",
            InterestIds = [1, 2]
        });
    }

    [Fact]
    public void ValidateCreatePreference_AllowsMissingBudget()
    {
        AiPreferenceOutputValidator.ValidateCreatePreference(new CreatePreferenceFromAiRequestDto
        {
            Duration = 7,
            TripType = "Nature",
            InterestIds = [1]
        });
    }
}
