using Microsoft.EntityFrameworkCore;
using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.DTOs.Activities;
using TravelRecommendation.Application.Interfaces;
using TravelRecommendation.Application.Validators;
using TravelRecommendation.Domain.Entities;
using TravelRecommendation.Infrastructure.Persistence;

namespace TravelRecommendation.Infrastructure.Services;

public class ActivityService : IActivityService
{
    private readonly TravelDbContext _dbContext;

    public ActivityService(TravelDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ActivityDto>> GetByDestinationAsync(
        int destinationId,
        CancellationToken cancellationToken = default)
    {
        await EnsureDestinationExistsAsync(destinationId, cancellationToken);

        return await _dbContext.Activities
            .AsNoTracking()
            .Where(a => a.DestinationId == destinationId)
            .OrderBy(a => a.Name)
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
            .ToListAsync(cancellationToken);
    }

    public async Task<ActivityDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var activity = await _dbContext.Activities
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (activity is null)
        {
            throw new NotFoundException($"Activity with id {id} was not found.");
        }

        return Map(activity);
    }

    public async Task<ActivityDto> CreateAsync(
        CreateActivityRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ContentValidators.ValidateActivityCreate(request);
        await EnsureDestinationExistsAsync(request.DestinationId, cancellationToken);

        var activity = new Activity
        {
            DestinationId = request.DestinationId,
            Name = request.Name.Trim(),
            Provider = request.Provider?.Trim(),
            Price = request.Price,
            Currency = request.Currency?.Trim(),
            ExternalUrl = request.ExternalUrl?.Trim(),
            Description = request.Description?.Trim()
        };

        _dbContext.Activities.Add(activity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(activity);
    }

    public async Task<ActivityDto> UpdateAsync(
        int id,
        UpdateActivityRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ContentValidators.ValidateActivityUpdate(request);

        var activity = await _dbContext.Activities
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (activity is null)
        {
            throw new NotFoundException($"Activity with id {id} was not found.");
        }

        activity.Name = request.Name.Trim();
        activity.Provider = request.Provider?.Trim();
        activity.Price = request.Price;
        activity.Currency = request.Currency?.Trim();
        activity.ExternalUrl = request.ExternalUrl?.Trim();
        activity.Description = request.Description?.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(activity);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var activity = await _dbContext.Activities
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (activity is null)
        {
            throw new NotFoundException($"Activity with id {id} was not found.");
        }

        _dbContext.Activities.Remove(activity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureDestinationExistsAsync(int destinationId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Destinations
            .AnyAsync(d => d.Id == destinationId, cancellationToken);

        if (!exists)
        {
            throw new NotFoundException($"Destination with id {destinationId} was not found.");
        }
    }

    private static ActivityDto Map(Activity activity)
    {
        return new ActivityDto
        {
            Id = activity.Id,
            DestinationId = activity.DestinationId,
            Name = activity.Name,
            Provider = activity.Provider,
            Price = activity.Price,
            Currency = activity.Currency,
            ExternalUrl = activity.ExternalUrl,
            Description = activity.Description
        };
    }
}
