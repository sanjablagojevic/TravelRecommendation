using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TravelRecommendation.Application.Ai;
using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.DTOs.Ai;
using TravelRecommendation.Application.Interfaces;
using TravelRecommendation.Infrastructure.Persistence;

namespace TravelRecommendation.Infrastructure.ExternalServices.OpenAI;

public class OpenAiRecommendationExplanationService : IAiRecommendationExplanationService
{
    private readonly TravelDbContext _dbContext;
    private readonly IOpenAiTextClient _openAiTextClient;
    private readonly ILogger<OpenAiRecommendationExplanationService> _logger;

    public OpenAiRecommendationExplanationService(
        TravelDbContext dbContext,
        IOpenAiTextClient openAiTextClient,
        ILogger<OpenAiRecommendationExplanationService> logger)
    {
        _dbContext = dbContext;
        _openAiTextClient = openAiTextClient;
        _logger = logger;
    }

    public async Task<AiExplanationsResponseDto> GenerateExplanationsAsync(
        int userId,
        int recommendationId,
        CancellationToken cancellationToken = default)
    {
        var recommendation = await _dbContext.Recommendations
            .Include(r => r.UserPreference)
                .ThenInclude(p => p.UserPreferenceInterests)
                    .ThenInclude(upi => upi.Interest)
            .Include(r => r.RecommendationItems)
                .ThenInclude(i => i.Destination)
                    .ThenInclude(d => d.DestinationInterests)
                        .ThenInclude(di => di.Interest)
            .FirstOrDefaultAsync(r => r.Id == recommendationId && r.UserId == userId, cancellationToken);

        if (recommendation is null)
        {
            throw new NotFoundException("Recommendation was not found.");
        }

        var preferredInterestNames = recommendation.UserPreference.UserPreferenceInterests
            .Select(x => x.Interest.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var items = recommendation.RecommendationItems
            .OrderBy(i => i.Rank)
            .ToList();

        var responseItems = new List<AiExplanationItemDto>();
        var usedFallback = false;
        string? warning = null;

        foreach (var item in items)
        {
            // Snapshot fields must remain untouched.
            var originalScore = item.Score;
            var originalRank = item.Rank;
            var originalEstimatedCost = item.EstimatedCost;
            var deterministic = item.Explanation;

            var destinationInterestNames = item.Destination.DestinationInterests
                .Select(di => di.Interest.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var matched = preferredInterestNames
                .Where(p => destinationInterestNames.Contains(p, StringComparer.OrdinalIgnoreCase))
                .ToList();
            var missing = preferredInterestNames
                .Where(p => !destinationInterestNames.Contains(p, StringComparer.OrdinalIgnoreCase))
                .ToList();

            string? aiText = item.AiExplanation;
            try
            {
                var payload = JsonSerializer.Serialize(new
                {
                    destination = new
                    {
                        item.Destination.Name,
                        item.Destination.Country
                    },
                    score = originalScore,
                    rank = originalRank,
                    estimatedCost = originalEstimatedCost,
                    user = new
                    {
                        recommendation.UserPreference.Budget,
                        recommendation.UserPreference.Duration,
                        recommendation.UserPreference.TripType,
                        recommendation.UserPreference.PreferredClimate
                    },
                    matchedInterests = matched,
                    missingInterests = missing,
                    deterministicExplanation = deterministic
                });

                aiText = await _openAiTextClient.CompleteTextAsync(
                    OpenAiJsonSchemas.BuildExplanationInstructions(),
                    payload,
                    cancellationToken);

                item.AiExplanation = aiText;
            }
            catch (Exception ex) when (ex is ServiceUnavailableAppException or InvalidOperationException)
            {
                usedFallback = true;
                warning = "OpenAI explanation unavailable. Deterministic explanations were kept as fallback.";
                _logger.LogWarning(ex, "AI explanation fallback used for recommendation {RecommendationId}.", recommendationId);
            }

            // Hard guarantee: scoring/ranking/cost never change.
            item.Score = originalScore;
            item.Rank = originalRank;
            item.EstimatedCost = originalEstimatedCost;
            item.Explanation = deterministic;

            responseItems.Add(new AiExplanationItemDto
            {
                RecommendationItemId = item.Id,
                DestinationId = item.DestinationId,
                Rank = item.Rank,
                Score = item.Score,
                EstimatedCost = item.EstimatedCost,
                DeterministicExplanation = item.Explanation,
                AiExplanation = item.AiExplanation
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AiExplanationsResponseDto
        {
            RecommendationId = recommendation.Id,
            UsedFallback = usedFallback,
            Warning = warning,
            Items = responseItems
        };
    }
}
