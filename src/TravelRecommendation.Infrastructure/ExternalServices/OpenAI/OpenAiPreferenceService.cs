using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TravelRecommendation.Application.Ai;
using TravelRecommendation.Application.DTOs.Ai;
using TravelRecommendation.Application.DTOs.UserPreferences;
using TravelRecommendation.Application.Interfaces;
using TravelRecommendation.Infrastructure.Persistence;

namespace TravelRecommendation.Infrastructure.ExternalServices.OpenAI;

public class OpenAiPreferenceService : IAiPreferenceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IOpenAiTextClient _openAiTextClient;
    private readonly TravelDbContext _dbContext;
    private readonly IUserPreferenceService _userPreferenceService;

    public OpenAiPreferenceService(
        IOpenAiTextClient openAiTextClient,
        TravelDbContext dbContext,
        IUserPreferenceService userPreferenceService)
    {
        _openAiTextClient = openAiTextClient;
        _dbContext = dbContext;
        _userPreferenceService = userPreferenceService;
    }

    public async Task<AiPreferenceExtractionResponseDto> ExtractPreferencesAsync(
        AiPreferenceRequestDto request,
        CancellationToken cancellationToken = default)
    {
        AiPreferenceOutputValidator.ValidateRequest(request);

        var json = await _openAiTextClient.CompleteJsonAsync(
            OpenAiJsonSchemas.BuildPreferenceExtractionInstructions(),
            request.Message.Trim(),
            OpenAiJsonSchemas.PreferenceExtractionSchemaName,
            OpenAiJsonSchemas.PreferenceExtractionSchema,
            cancellationToken);

        AiPreferenceExtractionDto? raw;
        try
        {
            raw = JsonSerializer.Deserialize<AiPreferenceExtractionDto>(json, JsonOptions);
        }
        catch (JsonException)
        {
            throw new Application.Common.Exceptions.ServiceUnavailableAppException(
                "OpenAI returned an invalid structured response.");
        }

        if (raw is null)
        {
            throw new Application.Common.Exceptions.ServiceUnavailableAppException(
                "OpenAI returned an empty structured response.");
        }

        var normalized = AiPreferenceOutputValidator.ValidateAndNormalize(raw);
        return await MapToResponseAsync(normalized, cancellationToken);
    }

    public async Task<CreatePreferenceFromAiResponseDto> CreatePreferenceAsync(
        int userId,
        CreatePreferenceFromAiRequestDto request,
        CancellationToken cancellationToken = default)
    {
        AiPreferenceOutputValidator.ValidateCreatePreference(request);

        var created = await _userPreferenceService.CreateAsync(
            userId,
            new CreateUserPreferenceRequestDto
            {
                Budget = request.Budget,
                Duration = request.Duration,
                TripType = request.TripType,
                TravelPeriod = request.TravelPeriod,
                PreferredClimate = request.PreferredClimate,
                MinTemperature = request.MinTemperature,
                MaxTemperature = request.MaxTemperature,
                InterestIds = request.InterestIds
            },
            cancellationToken);

        return new CreatePreferenceFromAiResponseDto
        {
            UserPreferenceId = created.Id
        };
    }

    private async Task<AiPreferenceExtractionResponseDto> MapToResponseAsync(
        AiPreferenceExtractionDto normalized,
        CancellationToken cancellationToken)
    {
        var interestsInDb = await _dbContext.Interests
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var mapped = new List<AiMappedInterestDto>();
        var unmapped = new List<string>(normalized.UnmappedPreferences);

        foreach (var interestName in normalized.Interests)
        {
            var entity = interestsInDb.FirstOrDefault(i =>
                string.Equals(i.Name, interestName, StringComparison.OrdinalIgnoreCase));

            if (entity is null)
            {
                if (!unmapped.Contains(interestName, StringComparer.OrdinalIgnoreCase))
                {
                    unmapped.Add(interestName);
                }

                continue;
            }

            mapped.Add(new AiMappedInterestDto
            {
                Id = entity.Id,
                Name = entity.Name
            });
        }

        var requiresConversion = normalized.Budget.HasValue
            && !string.IsNullOrWhiteSpace(normalized.BudgetCurrency)
            && !string.Equals(normalized.BudgetCurrency, "EUR", StringComparison.OrdinalIgnoreCase);

        return new AiPreferenceExtractionResponseDto
        {
            Budget = normalized.Budget,
            BudgetCurrency = normalized.BudgetCurrency,
            Duration = normalized.Duration,
            TripType = normalized.TripType,
            TravelPeriod = normalized.TravelPeriod,
            PreferredClimate = normalized.PreferredClimate,
            MinTemperature = normalized.MinTemperature,
            MaxTemperature = normalized.MaxTemperature,
            Interests = mapped,
            UnmappedPreferences = unmapped,
            AdditionalRequirements = normalized.AdditionalRequirements,
            RequiresCurrencyConversion = requiresConversion,
            CurrencyNote = requiresConversion
                ? "Budget currency is not EUR. Convert/confirm the amount in EUR before creating a preference. No FX rate was invented."
                : null
        };
    }
}
