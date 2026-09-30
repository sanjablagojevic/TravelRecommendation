using Microsoft.EntityFrameworkCore;
using TravelRecommendation.Application.Common;
using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.DTOs.Recommendations;
using TravelRecommendation.Application.Interfaces;
using TravelRecommendation.Application.Recommendations;
using TravelRecommendation.Domain.Entities;
using TravelRecommendation.Infrastructure.Persistence;

namespace TravelRecommendation.Infrastructure.Services;

public class RecommendationService : IRecommendationService
{
    private readonly TravelDbContext _dbContext;
    private readonly IRecommendationScoringService _scoringService;

    public RecommendationService(
        TravelDbContext dbContext,
        IRecommendationScoringService scoringService)
    {
        _dbContext = dbContext;
        _scoringService = scoringService;
    }

    public async Task<RecommendationDto> GenerateAsync(
        int userId,
        GenerateRecommendationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var topCount = NormalizeTopCount(request.TopCount);

        var preference = await _dbContext.UserPreferences
            .AsNoTracking()
            .Include(p => p.UserPreferenceInterests)
                .ThenInclude(upi => upi.Interest)
            .FirstOrDefaultAsync(
                p => p.Id == request.UserPreferenceId && p.UserId == userId,
                cancellationToken);

        if (preference is null)
        {
            throw new NotFoundException("User preference was not found.");
        }

        var destinations = await _dbContext.Destinations
            .AsNoTracking()
            .Where(d => d.IsActive)
            .Include(d => d.DestinationInterests)
                .ThenInclude(di => di.Interest)
            .Include(d => d.DestinationCategories)
                .ThenInclude(dc => dc.Category)
            .ToListAsync(cancellationToken);

        if (destinations.Count == 0)
        {
            throw new NotFoundException("No active destinations are available for recommendation.");
        }

        var preferenceInput = new PreferenceScoringInput
        {
            Budget = preference.Budget,
            Duration = preference.Duration,
            TripType = preference.TripType,
            PreferredClimate = preference.PreferredClimate,
            InterestNames = preference.UserPreferenceInterests
                .Select(x => x.Interest.Name)
                .ToList()
        };

        var destinationInputs = destinations.Select(d => new DestinationScoringInput
        {
            Id = d.Id,
            Name = d.Name,
            Country = d.Country,
            City = d.City,
            AverageDailyCost = d.AverageDailyCost,
            Popularity = d.Popularity,
            Climate = d.Climate,
            ImageUrl = d.ImageUrl,
            InterestNames = d.DestinationInterests.Select(di => di.Interest.Name).ToList(),
            CategoryNames = d.DestinationCategories.Select(dc => dc.Category.Name).ToList()
        }).ToList();

        var ranked = _scoringService.RankDestinations(preferenceInput, destinationInputs, topCount);
        var destinationById = destinations.ToDictionary(d => d.Id);

        var recommendation = new Recommendation
        {
            UserId = userId,
            UserPreferenceId = preference.Id,
            OriginalQuery = null,
            CreatedAt = DateTime.UtcNow
        };

        var rank = 1;
        foreach (var scoreResult in ranked)
        {
            recommendation.RecommendationItems.Add(new RecommendationItem
            {
                DestinationId = scoreResult.DestinationId,
                Score = scoreResult.FinalScore,
                Rank = rank,
                Explanation = scoreResult.Explanation,
                EstimatedCost = scoreResult.EstimatedCost
            });
            rank++;
        }

        _dbContext.Recommendations.Add(recommendation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var items = ranked.Select((scoreResult, index) =>
        {
            var destination = destinationById[scoreResult.DestinationId];
            return new RecommendationItemDto
            {
                Rank = index + 1,
                Destination = MapDestination(destination),
                Score = scoreResult.FinalScore,
                MatchPercentage = Math.Round(scoreResult.FinalScore * 100m, 2, MidpointRounding.AwayFromZero),
                EstimatedCost = scoreResult.EstimatedCost,
                MatchedInterests = scoreResult.MatchedInterests,
                MissingInterests = scoreResult.MissingInterests,
                Explanation = scoreResult.Explanation,
                AiExplanation = null,
                ScoreBreakdown = scoreResult.Breakdown
            };
        }).ToList();

        return new RecommendationDto
        {
            Id = recommendation.Id,
            CreatedAt = recommendation.CreatedAt,
            UserPreferenceId = recommendation.UserPreferenceId,
            Items = items
        };
    }

    public async Task<RecommendationDto> GetByIdAsync(
        int userId,
        int recommendationId,
        CancellationToken cancellationToken = default)
    {
        var recommendation = await _dbContext.Recommendations
            .AsNoTracking()
            .Include(r => r.UserPreference)
                .ThenInclude(p => p.UserPreferenceInterests)
                    .ThenInclude(upi => upi.Interest)
            .Include(r => r.RecommendationItems)
                .ThenInclude(i => i.Destination)
                    .ThenInclude(d => d.DestinationInterests)
                        .ThenInclude(di => di.Interest)
            .FirstOrDefaultAsync(
                r => r.Id == recommendationId && r.UserId == userId,
                cancellationToken);

        if (recommendation is null)
        {
            throw new NotFoundException("Recommendation was not found.");
        }

        var preferredInterestNames = recommendation.UserPreference.UserPreferenceInterests
            .Select(x => x.Interest.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Historical GET uses stored Score/Rank/EstimatedCost/Explanation snapshots.
        // ScoreBreakdown is omitted because it is not persisted and current destination
        // data could diverge from the original calculation.
        // Matched/Missing interests are derived from current catalog tags for display only.
        var items = recommendation.RecommendationItems
            .OrderBy(i => i.Rank)
            .Select(i =>
            {
                var destinationInterestNames = i.Destination.DestinationInterests
                    .Select(di => di.Interest.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var matched = preferredInterestNames
                    .Where(p => destinationInterestNames.Contains(p, StringComparer.OrdinalIgnoreCase))
                    .ToList();
                var missing = preferredInterestNames
                    .Where(p => !destinationInterestNames.Contains(p, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                return new RecommendationItemDto
                {
                    Rank = i.Rank,
                    Destination = MapDestination(i.Destination),
                    Score = i.Score,
                    MatchPercentage = Math.Round(i.Score * 100m, 2, MidpointRounding.AwayFromZero),
                    EstimatedCost = i.EstimatedCost,
                    MatchedInterests = matched,
                    MissingInterests = missing,
                    Explanation = i.Explanation ?? string.Empty,
                    AiExplanation = i.AiExplanation,
                    ScoreBreakdown = null
                };
            })
            .ToList();

        return new RecommendationDto
        {
            Id = recommendation.Id,
            CreatedAt = recommendation.CreatedAt,
            UserPreferenceId = recommendation.UserPreferenceId,
            Items = items
        };
    }

    public async Task<PagedResult<RecommendationHistoryDto>> GetHistoryAsync(
        int userId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 10;
        }

        if (pageSize > 100)
        {
            pageSize = 100;
        }

        var query = _dbContext.Recommendations
            .AsNoTracking()
            .Where(r => r.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RecommendationHistoryDto
            {
                Id = r.Id,
                CreatedAt = r.CreatedAt,
                UserPreferenceId = r.UserPreferenceId,
                NumberOfResults = r.RecommendationItems.Count,
                TopScore = r.RecommendationItems
                    .OrderBy(i => i.Rank)
                    .Select(i => (decimal?)i.Score)
                    .FirstOrDefault(),
                TopDestination = r.RecommendationItems
                    .OrderBy(i => i.Rank)
                    .Select(i => i.Destination.Name)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return PagedResult<RecommendationHistoryDto>.Create(items, page, pageSize, totalCount);
    }

    private static int NormalizeTopCount(int? topCount)
    {
        var value = topCount ?? 5;
        if (value < 1)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["TopCount"] = ["TopCount must be at least 1."]
            });
        }

        if (value > 10)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["TopCount"] = ["TopCount must be at most 10."]
            });
        }

        return value;
    }

    private static RecommendationDestinationDto MapDestination(Destination destination)
    {
        return new RecommendationDestinationDto
        {
            Id = destination.Id,
            Name = destination.Name,
            Country = destination.Country,
            City = destination.City,
            ImageUrl = destination.ImageUrl,
            AverageDailyCost = destination.AverageDailyCost,
            Climate = destination.Climate
        };
    }
}
