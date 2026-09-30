using Microsoft.EntityFrameworkCore;
using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.DTOs.UserPreferences;
using TravelRecommendation.Application.Interfaces;
using TravelRecommendation.Application.Validators;
using TravelRecommendation.Domain.Entities;
using TravelRecommendation.Infrastructure.Persistence;

namespace TravelRecommendation.Infrastructure.Services;

public class UserPreferenceService : IUserPreferenceService
{
    private readonly TravelDbContext _dbContext;

    public UserPreferenceService(TravelDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserPreferenceDetailsDto> CreateAsync(
        int userId,
        CreateUserPreferenceRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var interestIds = PreferenceValidators.ValidateCreate(request);
        await EnsureInterestsExistAsync(interestIds, cancellationToken);

        var preference = new UserPreference
        {
            UserId = userId,
            Budget = request.Budget,
            Duration = request.Duration,
            TripType = NormalizeOptional(request.TripType),
            TravelPeriod = NormalizeOptional(request.TravelPeriod),
            PreferredClimate = NormalizeOptional(request.PreferredClimate),
            MinTemperature = request.MinTemperature,
            MaxTemperature = request.MaxTemperature,
            CreatedAt = DateTime.UtcNow,
            UserPreferenceInterests = interestIds
                .Select(id => new UserPreferenceInterest { InterestId = id })
                .ToList()
        };

        _dbContext.UserPreferences.Add(preference);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(userId, preference.Id, cancellationToken);
    }

    public async Task<UserPreferenceDetailsDto> GetByIdAsync(
        int userId,
        int preferenceId,
        CancellationToken cancellationToken = default)
    {
        var preference = await _dbContext.UserPreferences
            .AsNoTracking()
            .Include(p => p.UserPreferenceInterests)
                .ThenInclude(upi => upi.Interest)
            .FirstOrDefaultAsync(p => p.Id == preferenceId && p.UserId == userId, cancellationToken);

        if (preference is null)
        {
            throw new NotFoundException("User preference was not found.");
        }

        return MapDetails(preference);
    }

    public async Task<IReadOnlyList<UserPreferenceDto>> GetUserPreferencesAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var preferences = await _dbContext.UserPreferences
            .AsNoTracking()
            .Include(p => p.UserPreferenceInterests)
                .ThenInclude(upi => upi.Interest)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return preferences.Select(MapList).ToList();
    }

    public async Task<UserPreferenceDetailsDto> UpdateAsync(
        int userId,
        int preferenceId,
        UpdateUserPreferenceRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var interestIds = PreferenceValidators.ValidateUpdate(request);
        await EnsureInterestsExistAsync(interestIds, cancellationToken);

        var preference = await _dbContext.UserPreferences
            .Include(p => p.UserPreferenceInterests)
            .FirstOrDefaultAsync(p => p.Id == preferenceId && p.UserId == userId, cancellationToken);

        if (preference is null)
        {
            throw new NotFoundException("User preference was not found.");
        }

        preference.Budget = request.Budget;
        preference.Duration = request.Duration;
        preference.TripType = NormalizeOptional(request.TripType);
        preference.TravelPeriod = NormalizeOptional(request.TravelPeriod);
        preference.PreferredClimate = NormalizeOptional(request.PreferredClimate);
        preference.MinTemperature = request.MinTemperature;
        preference.MaxTemperature = request.MaxTemperature;

        SyncInterests(preference, interestIds);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(userId, preference.Id, cancellationToken);
    }

    public async Task DeleteAsync(
        int userId,
        int preferenceId,
        CancellationToken cancellationToken = default)
    {
        var preference = await _dbContext.UserPreferences
            .Include(p => p.UserPreferenceInterests)
            .FirstOrDefaultAsync(p => p.Id == preferenceId && p.UserId == userId, cancellationToken);

        if (preference is null)
        {
            throw new NotFoundException("User preference was not found.");
        }

        // Keep historical Recommendation rows intact — do not cascade-delete preferences in use.
        var hasRecommendations = await _dbContext.Recommendations
            .AnyAsync(r => r.UserPreferenceId == preferenceId, cancellationToken);

        if (hasRecommendations)
        {
            throw new ConflictException(
                "Preference cannot be deleted because it is referenced by historical recommendations.");
        }

        _dbContext.UserPreferenceInterests.RemoveRange(preference.UserPreferenceInterests);
        _dbContext.UserPreferences.Remove(preference);
        await _dbContext.SaveChangesAsync(cancellationToken);
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

    private static void SyncInterests(UserPreference preference, List<int> interestIds)
    {
        var existing = preference.UserPreferenceInterests.ToList();

        foreach (var link in existing.Where(x => !interestIds.Contains(x.InterestId)))
        {
            preference.UserPreferenceInterests.Remove(link);
        }

        var existingIds = preference.UserPreferenceInterests.Select(x => x.InterestId).ToHashSet();
        foreach (var interestId in interestIds.Where(id => !existingIds.Contains(id)))
        {
            preference.UserPreferenceInterests.Add(new UserPreferenceInterest
            {
                UserPreferenceId = preference.Id,
                InterestId = interestId
            });
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static UserPreferenceDetailsDto MapDetails(UserPreference preference)
    {
        return new UserPreferenceDetailsDto
        {
            Id = preference.Id,
            Budget = preference.Budget,
            Duration = preference.Duration,
            TripType = preference.TripType,
            TravelPeriod = preference.TravelPeriod,
            PreferredClimate = preference.PreferredClimate,
            MinTemperature = preference.MinTemperature,
            MaxTemperature = preference.MaxTemperature,
            CreatedAt = preference.CreatedAt,
            Interests = preference.UserPreferenceInterests
                .Select(x => new PreferenceInterestDto
                {
                    Id = x.Interest.Id,
                    Name = x.Interest.Name
                })
                .OrderBy(x => x.Name)
                .ToList()
        };
    }

    private static UserPreferenceDto MapList(UserPreference preference)
    {
        return new UserPreferenceDto
        {
            Id = preference.Id,
            Budget = preference.Budget,
            Duration = preference.Duration,
            TripType = preference.TripType,
            TravelPeriod = preference.TravelPeriod,
            PreferredClimate = preference.PreferredClimate,
            MinTemperature = preference.MinTemperature,
            MaxTemperature = preference.MaxTemperature,
            CreatedAt = preference.CreatedAt,
            Interests = preference.UserPreferenceInterests
                .Select(x => new PreferenceInterestDto
                {
                    Id = x.Interest.Id,
                    Name = x.Interest.Name
                })
                .OrderBy(x => x.Name)
                .ToList()
        };
    }
}
