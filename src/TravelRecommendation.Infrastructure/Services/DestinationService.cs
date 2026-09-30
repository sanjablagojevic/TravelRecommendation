using Microsoft.EntityFrameworkCore;
using TravelRecommendation.Application.Common;
using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.DTOs.Activities;
using TravelRecommendation.Application.DTOs.Attractions;
using TravelRecommendation.Application.DTOs.Categories;
using TravelRecommendation.Application.DTOs.Destinations;
using TravelRecommendation.Application.Interfaces;
using TravelRecommendation.Application.Validators;
using TravelRecommendation.Domain.Entities;
using TravelRecommendation.Infrastructure.ExternalServices.Weather.OpenWeather;
using TravelRecommendation.Infrastructure.Persistence;

namespace TravelRecommendation.Infrastructure.Services;

public class DestinationService : IDestinationService
{
    private readonly TravelDbContext _dbContext;

    public DestinationService(TravelDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<DestinationListDto>> GetAllAsync(
        DestinationQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        ContentValidators.NormalizePaging(query);

        var destinations = _dbContext.Destinations
            .AsNoTracking()
            .Include(d => d.DestinationCategories)
                .ThenInclude(dc => dc.Category)
            .Include(d => d.DestinationInterests)
                .ThenInclude(di => di.Interest)
            .AsQueryable();

        if (query.IsActive.HasValue)
        {
            destinations = destinations.Where(d => d.IsActive == query.IsActive.Value);
        }
        else if (!query.IncludeInactive)
        {
            destinations = destinations.Where(d => d.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            destinations = destinations.Where(d =>
                EF.Functions.Like(d.Name, $"%{search}%") ||
                EF.Functions.Like(d.Country, $"%{search}%") ||
                EF.Functions.Like(d.City, $"%{search}%"));
        }

        if (!string.IsNullOrWhiteSpace(query.Country))
        {
            var country = query.Country.Trim();
            destinations = destinations.Where(d => d.Country == country);
        }

        if (!string.IsNullOrWhiteSpace(query.City))
        {
            var city = query.City.Trim();
            destinations = destinations.Where(d => d.City == city);
        }

        if (query.CategoryId.HasValue)
        {
            var categoryId = query.CategoryId.Value;
            destinations = destinations.Where(d =>
                d.DestinationCategories.Any(dc => dc.CategoryId == categoryId));
        }

        if (query.InterestId.HasValue)
        {
            var interestId = query.InterestId.Value;
            destinations = destinations.Where(d =>
                d.DestinationInterests.Any(di => di.InterestId == interestId));
        }

        var totalCount = await destinations.CountAsync(cancellationToken);

        var items = await destinations
            .OrderByDescending(d => d.Popularity)
            .ThenBy(d => d.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(d => new DestinationListDto
            {
                Id = d.Id,
                Name = d.Name,
                Country = d.Country,
                City = d.City,
                AverageDailyCost = d.AverageDailyCost,
                Popularity = d.Popularity,
                ImageUrl = d.ImageUrl,
                Climate = d.Climate,
                IsActive = d.IsActive,
                Categories = d.DestinationCategories
                    .Select(dc => new CategoryDto
                    {
                        Id = dc.Category.Id,
                        Name = dc.Category.Name,
                        Type = dc.Category.Type,
                        Description = dc.Category.Description
                    })
                    .ToList(),
                Interests = d.DestinationInterests
                    .Select(di => new DestinationInterestDto
                    {
                        Id = di.Interest.Id,
                        Name = di.Interest.Name
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return PagedResult<DestinationListDto>.Create(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<DestinationDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var destination = await _dbContext.Destinations
            .AsNoTracking()
            .Include(d => d.DestinationCategories)
                .ThenInclude(dc => dc.Category)
            .Include(d => d.DestinationInterests)
                .ThenInclude(di => di.Interest)
            .Include(d => d.Attractions)
            .Include(d => d.Activities)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (destination is null)
        {
            throw new NotFoundException($"Destination with id {id} was not found.");
        }

        return MapToDetails(destination);
    }

    public async Task<DestinationDetailsDto> CreateAsync(
        CreateDestinationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ContentValidators.ValidateDestinationCreate(request);

        var categoryIds = request.CategoryIds.Distinct().ToList();
        var interestIds = request.InterestIds.Distinct().ToList();
        await EnsureCategoriesExistAsync(categoryIds, cancellationToken);
        await EnsureInterestsExistAsync(interestIds, cancellationToken);
        await EnsureDestinationUniqueAsync(request.Name, request.Country, request.City, excludeId: null, cancellationToken);

        var destination = new Destination
        {
            Name = request.Name.Trim(),
            Country = request.Country.Trim(),
            City = request.City.Trim(),
            Description = request.Description?.Trim(),
            AverageDailyCost = request.AverageDailyCost,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Popularity = request.Popularity,
            ImageUrl = request.ImageUrl?.Trim(),
            Climate = request.Climate?.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            DestinationCategories = categoryIds
                .Select(categoryId => new DestinationCategory { CategoryId = categoryId })
                .ToList(),
            DestinationInterests = interestIds
                .Select(interestId => new DestinationInterest { InterestId = interestId })
                .ToList()
        };

        _dbContext.Destinations.Add(destination);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(destination.Id, cancellationToken);
    }

    public async Task<DestinationDetailsDto> UpdateAsync(
        int id,
        UpdateDestinationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ContentValidators.ValidateDestinationUpdate(request);

        var destination = await _dbContext.Destinations
            .Include(d => d.DestinationCategories)
            .Include(d => d.DestinationInterests)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (destination is null)
        {
            throw new NotFoundException($"Destination with id {id} was not found.");
        }

        var categoryIds = request.CategoryIds.Distinct().ToList();
        var interestIds = request.InterestIds.Distinct().ToList();
        await EnsureCategoriesExistAsync(categoryIds, cancellationToken);
        await EnsureInterestsExistAsync(interestIds, cancellationToken);
        await EnsureDestinationUniqueAsync(request.Name, request.Country, request.City, id, cancellationToken);

        var coordinatesChanged =
            destination.Latitude != request.Latitude ||
            destination.Longitude != request.Longitude;

        destination.Name = request.Name.Trim();
        destination.Country = request.Country.Trim();
        destination.City = request.City.Trim();
        destination.Description = request.Description?.Trim();
        destination.AverageDailyCost = request.AverageDailyCost;
        destination.Latitude = request.Latitude;
        destination.Longitude = request.Longitude;
        destination.Popularity = request.Popularity;
        destination.ImageUrl = request.ImageUrl?.Trim();
        destination.Climate = request.Climate?.Trim();
        destination.IsActive = request.IsActive;

        SyncCategories(destination, categoryIds);
        SyncInterests(destination, interestIds);

        if (coordinatesChanged)
        {
            await InvalidateWeatherCacheAsync(destination.Id, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(destination.Id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var destination = await _dbContext.Destinations
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (destination is null)
        {
            throw new NotFoundException($"Destination with id {id} was not found.");
        }

        destination.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task InvalidateWeatherCacheAsync(int destinationId, CancellationToken cancellationToken)
    {
        var requestKey = OpenWeatherService.BuildCacheKey(destinationId);
        var cacheEntries = await _dbContext.ApiCaches
            .Where(c => c.Action == OpenWeatherService.CacheAction && c.RequestKey == requestKey)
            .ToListAsync(cancellationToken);

        if (cacheEntries.Count == 0)
        {
            return;
        }

        _dbContext.ApiCaches.RemoveRange(cacheEntries);
    }

    private static void SyncCategories(Destination destination, List<int> categoryIds)
    {
        var existing = destination.DestinationCategories.ToList();

        foreach (var link in existing.Where(x => !categoryIds.Contains(x.CategoryId)))
        {
            destination.DestinationCategories.Remove(link);
        }

        var existingIds = destination.DestinationCategories.Select(x => x.CategoryId).ToHashSet();
        foreach (var categoryId in categoryIds.Where(id => !existingIds.Contains(id)))
        {
            destination.DestinationCategories.Add(new DestinationCategory
            {
                DestinationId = destination.Id,
                CategoryId = categoryId
            });
        }
    }

    private static void SyncInterests(Destination destination, List<int> interestIds)
    {
        var existing = destination.DestinationInterests.ToList();

        foreach (var link in existing.Where(x => !interestIds.Contains(x.InterestId)))
        {
            destination.DestinationInterests.Remove(link);
        }

        var existingIds = destination.DestinationInterests.Select(x => x.InterestId).ToHashSet();
        foreach (var interestId in interestIds.Where(id => !existingIds.Contains(id)))
        {
            destination.DestinationInterests.Add(new DestinationInterest
            {
                DestinationId = destination.Id,
                InterestId = interestId
            });
        }
    }

    private async Task EnsureCategoriesExistAsync(List<int> categoryIds, CancellationToken cancellationToken)
    {
        if (categoryIds.Count == 0)
        {
            return;
        }

        var existingCount = await _dbContext.Categories
            .CountAsync(c => categoryIds.Contains(c.Id), cancellationToken);

        if (existingCount != categoryIds.Count)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["CategoryIds"] = ["One or more CategoryIds do not exist."]
            });
        }
    }

    private async Task EnsureInterestsExistAsync(List<int> interestIds, CancellationToken cancellationToken)
    {
        if (interestIds.Count == 0)
        {
            return;
        }

        var existingCount = await _dbContext.Interests
            .CountAsync(i => interestIds.Contains(i.Id), cancellationToken);

        if (existingCount != interestIds.Count)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["InterestIds"] = ["One or more InterestIds do not exist."]
            });
        }
    }

    private async Task EnsureDestinationUniqueAsync(
        string name,
        string country,
        string city,
        int? excludeId,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim();
        var normalizedCountry = country.Trim();
        var normalizedCity = city.Trim();

        var exists = await _dbContext.Destinations.AnyAsync(
            d => d.Name == normalizedName
                 && d.Country == normalizedCountry
                 && d.City == normalizedCity
                 && (!excludeId.HasValue || d.Id != excludeId.Value),
            cancellationToken);

        if (exists)
        {
            throw new ConflictException(
                $"Destination '{normalizedName}' in {normalizedCity}, {normalizedCountry} already exists.");
        }
    }

    private static DestinationDetailsDto MapToDetails(Destination destination)
    {
        return new DestinationDetailsDto
        {
            Id = destination.Id,
            Name = destination.Name,
            Country = destination.Country,
            City = destination.City,
            Description = destination.Description,
            AverageDailyCost = destination.AverageDailyCost,
            Latitude = destination.Latitude,
            Longitude = destination.Longitude,
            Popularity = destination.Popularity,
            ImageUrl = destination.ImageUrl,
            Climate = destination.Climate,
            IsActive = destination.IsActive,
            CreatedAt = destination.CreatedAt,
            Categories = destination.DestinationCategories
                .Select(dc => new CategoryDto
                {
                    Id = dc.Category.Id,
                    Name = dc.Category.Name,
                    Type = dc.Category.Type,
                    Description = dc.Category.Description
                })
                .ToList(),
            Interests = destination.DestinationInterests
                .Select(di => new DestinationInterestDto
                {
                    Id = di.Interest.Id,
                    Name = di.Interest.Name
                })
                .ToList(),
            Attractions = destination.Attractions
                .Select(a => new AttractionDto
                {
                    Id = a.Id,
                    DestinationId = a.DestinationId,
                    Name = a.Name,
                    Description = a.Description,
                    Location = a.Location,
                    Latitude = a.Latitude,
                    Longitude = a.Longitude,
                    ImageUrl = a.ImageUrl
                })
                .ToList(),
            Activities = destination.Activities
                .Select(a => new ActivityDto
                {
                    Id = a.Id,
                    DestinationId = a.DestinationId,
                    Name = a.Name,
                    Provider = a.Provider,
                    Price = a.Price,
                    Currency = a.Currency,
                    ExternalUrl = a.ExternalUrl,
                    Description = a.Description
                })
                .ToList()
        };
    }
}
