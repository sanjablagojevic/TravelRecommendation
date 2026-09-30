using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TravelRecommendation.Application.Common;
using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.DTOs.Itineraries;
using TravelRecommendation.Application.Interfaces;
using TravelRecommendation.Application.Itineraries;
using TravelRecommendation.Application.Settings;
using TravelRecommendation.Domain.Entities;
using TravelRecommendation.Domain.Enums;
using TravelRecommendation.Infrastructure.Persistence;

namespace TravelRecommendation.Infrastructure.Services;

public class ItineraryService : IItineraryService
{
    private readonly TravelDbContext _dbContext;
    private readonly IAiItineraryService _aiItineraryService;
    private readonly OpenAiSettings _openAiSettings;
    private readonly ILogger<ItineraryService> _logger;

    public ItineraryService(
        TravelDbContext dbContext,
        IAiItineraryService aiItineraryService,
        IOptions<OpenAiSettings> openAiSettings,
        ILogger<ItineraryService> logger)
    {
        _dbContext = dbContext;
        _aiItineraryService = aiItineraryService;
        _openAiSettings = openAiSettings.Value;
        _logger = logger;
    }

    public async Task<TravelItineraryDto> GenerateAsync(
        int userId,
        GenerateItineraryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        AiItineraryOutputValidator.ValidateRequest(request);

        var destination = await _dbContext.Destinations
            .AsNoTracking()
            .Include(d => d.DestinationCategories).ThenInclude(dc => dc.Category)
            .Include(d => d.DestinationInterests).ThenInclude(di => di.Interest)
            .Include(d => d.Attractions)
            .Include(d => d.Activities)
            .FirstOrDefaultAsync(d => d.Id == request.DestinationId && d.IsActive, cancellationToken);

        if (destination is null)
        {
            throw new NotFoundException("Destination was not found.");
        }

        string? tripType = null;
        string? travelPeriod = null;
        string? preferredClimate = null;
        var preferenceInterests = new List<string>();
        int? preferenceId = request.UserPreferenceId;
        int? recommendationId = request.RecommendationId;

        if (recommendationId.HasValue)
        {
            var recommendation = await _dbContext.Recommendations
                .AsNoTracking()
                .Include(r => r.RecommendationItems)
                .Include(r => r.UserPreference)
                    .ThenInclude(p => p.UserPreferenceInterests)
                        .ThenInclude(upi => upi.Interest)
                .FirstOrDefaultAsync(r => r.Id == recommendationId.Value, cancellationToken);

            if (recommendation is null || recommendation.UserId != userId)
            {
                throw new NotFoundException("Recommendation was not found.");
            }

            if (!recommendation.RecommendationItems.Any(i => i.DestinationId == request.DestinationId))
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    ["DestinationId"] = ["Destination is not part of the selected recommendation."]
                });
            }

            preferenceId ??= recommendation.UserPreferenceId;
            ApplyPreference(recommendation.UserPreference, ref tripType, ref travelPeriod, ref preferredClimate, preferenceInterests);
        }

        if (preferenceId.HasValue)
        {
            var preference = await _dbContext.UserPreferences
                .AsNoTracking()
                .Include(p => p.UserPreferenceInterests)
                    .ThenInclude(upi => upi.Interest)
                .FirstOrDefaultAsync(p => p.Id == preferenceId.Value, cancellationToken);

            if (preference is null || preference.UserId != userId)
            {
                throw new NotFoundException("User preference was not found.");
            }

            ApplyPreference(preference, ref tripType, ref travelPeriod, ref preferredClimate, preferenceInterests);
        }

        var attractionIds = destination.Attractions.Select(a => a.Id).ToHashSet();
        var activityIds = destination.Activities.Select(a => a.Id).ToHashSet();

        var context = new AiItineraryGenerationContext
        {
            DestinationId = destination.Id,
            DestinationName = destination.Name,
            City = destination.City,
            Country = destination.Country,
            Description = destination.Description,
            Climate = destination.Climate,
            Categories = destination.DestinationCategories.Select(dc => dc.Category.Name).ToList(),
            Interests = destination.DestinationInterests.Select(di => di.Interest.Name).ToList(),
            Attractions = destination.Attractions.Select(a => new AiItineraryCatalogItem
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                Location = a.Location
            }).ToList(),
            Activities = destination.Activities.Select(a => new AiItineraryCatalogItem
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                Price = a.Price,
                Currency = a.Currency
            }).ToList(),
            DurationDays = request.DurationDays,
            TripType = tripType,
            TravelPeriod = travelPeriod,
            PreferredClimate = preferredClimate,
            PreferenceInterests = preferenceInterests,
            AdditionalRequest = request.AdditionalRequest
        };

        AiGeneratedItineraryDto generated;
        try
        {
            generated = await _aiItineraryService.GenerateAsync(context, cancellationToken);
        }
        catch (ServiceUnavailableAppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected itinerary AI failure for DestinationId={DestinationId}", destination.Id);
            throw new ServiceUnavailableAppException("Itinerary generation is temporarily unavailable.");
        }

        AiGeneratedItineraryDto normalized;
        try
        {
            normalized = AiItineraryOutputValidator.ValidateAndNormalize(
                generated,
                request.DurationDays,
                attractionIds,
                activityIds);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Rejected invalid itinerary AI output for DestinationId={DestinationId}: {Reason}",
                destination.Id,
                ex.Message);
            throw new ServiceUnavailableAppException("Itinerary generation is temporarily unavailable.");
        }

        var entity = new TravelItinerary
        {
            UserId = userId,
            DestinationId = destination.Id,
            UserPreferenceId = preferenceId,
            RecommendationId = recommendationId,
            Title = normalized.Title,
            DurationDays = request.DurationDays,
            CreatedAt = DateTime.UtcNow,
            AiModel = string.IsNullOrWhiteSpace(_openAiSettings.Model) ? null : _openAiSettings.Model.Trim(),
            Days = normalized.Days.Select(day => new TravelItineraryDay
            {
                DayNumber = day.DayNumber,
                Title = day.Title,
                Summary = day.Summary,
                Items = day.Items.Select(item => new TravelItineraryItem
                {
                    Order = item.Order,
                    TimeOfDay = item.TimeOfDay,
                    Title = item.Title,
                    Description = item.Description,
                    ItemType = Enum.Parse<ItineraryItemType>(item.ItemType, ignoreCase: true),
                    AttractionId = item.AttractionId,
                    ActivityId = item.ActivityId,
                    Location = item.Location
                }).ToList()
            }).ToList()
        };

        _dbContext.TravelItineraries.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(userId, entity.Id, cancellationToken);
    }

    public async Task<TravelItineraryDto> GetByIdAsync(
        int userId,
        int id,
        CancellationToken cancellationToken = default)
    {
        var itinerary = await _dbContext.TravelItineraries
            .AsNoTracking()
            .Include(i => i.Destination)
            .Include(i => i.Days)
                .ThenInclude(d => d.Items)
                    .ThenInclude(item => item.Attraction)
            .Include(i => i.Days)
                .ThenInclude(d => d.Items)
                    .ThenInclude(item => item.Activity)
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId, cancellationToken);

        if (itinerary is null)
        {
            throw new NotFoundException("Itinerary was not found.");
        }

        return MapDetails(itinerary);
    }

    public async Task<PagedResult<ItineraryListDto>> GetUserItinerariesAsync(
        int userId,
        int page,
        int pageSize,
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

        if (pageSize > 50)
        {
            pageSize = 50;
        }

        var query = _dbContext.TravelItineraries
            .AsNoTracking()
            .Where(i => i.UserId == userId);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new ItineraryListDto
            {
                Id = i.Id,
                Title = i.Title,
                DestinationId = i.DestinationId,
                DestinationName = i.Destination.Name,
                DestinationImageUrl = i.Destination.ImageUrl,
                DurationDays = i.DurationDays,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return PagedResult<ItineraryListDto>.Create(items, page, pageSize, total);
    }

    public async Task DeleteAsync(int userId, int id, CancellationToken cancellationToken = default)
    {
        var itinerary = await _dbContext.TravelItineraries
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId, cancellationToken);

        if (itinerary is null)
        {
            throw new NotFoundException("Itinerary was not found.");
        }

        _dbContext.TravelItineraries.Remove(itinerary);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void ApplyPreference(
        UserPreference preference,
        ref string? tripType,
        ref string? travelPeriod,
        ref string? preferredClimate,
        List<string> preferenceInterests)
    {
        tripType ??= preference.TripType;
        travelPeriod ??= preference.TravelPeriod;
        preferredClimate ??= preference.PreferredClimate;

        foreach (var name in preference.UserPreferenceInterests.Select(x => x.Interest.Name))
        {
            if (!preferenceInterests.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                preferenceInterests.Add(name);
            }
        }
    }

    private static TravelItineraryDto MapDetails(TravelItinerary itinerary)
    {
        return new TravelItineraryDto
        {
            Id = itinerary.Id,
            Title = itinerary.Title,
            DurationDays = itinerary.DurationDays,
            CreatedAt = itinerary.CreatedAt,
            Destination = new ItineraryDestinationSummaryDto
            {
                Id = itinerary.Destination.Id,
                Name = itinerary.Destination.Name,
                City = itinerary.Destination.City,
                Country = itinerary.Destination.Country,
                ImageUrl = itinerary.Destination.ImageUrl
            },
            Days = itinerary.Days
                .OrderBy(d => d.DayNumber)
                .Select(d => new TravelItineraryDayDto
                {
                    DayNumber = d.DayNumber,
                    Title = d.Title,
                    Summary = d.Summary,
                    Items = d.Items
                        .OrderBy(i => i.Order)
                        .Select(i => new TravelItineraryItemDto
                        {
                            Order = i.Order,
                            TimeOfDay = i.TimeOfDay,
                            Title = i.Title,
                            Description = i.Description,
                            ItemType = i.ItemType.ToString(),
                            AttractionId = i.AttractionId,
                            AttractionName = i.Attraction?.Name,
                            AttractionImageUrl = i.Attraction?.ImageUrl,
                            ActivityId = i.ActivityId,
                            ActivityName = i.Activity?.Name,
                            ActivityPrice = i.Activity?.Price,
                            ActivityCurrency = i.Activity?.Currency,
                            Location = i.Location
                        })
                        .ToList()
                })
                .ToList()
        };
    }
}
