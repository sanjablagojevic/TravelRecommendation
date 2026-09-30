using Microsoft.EntityFrameworkCore;
using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.DTOs.Interests;
using TravelRecommendation.Application.Interfaces;
using TravelRecommendation.Application.Validators;
using TravelRecommendation.Domain.Entities;
using TravelRecommendation.Infrastructure.Persistence;

namespace TravelRecommendation.Infrastructure.Services;

public class InterestService : IInterestService
{
    private readonly TravelDbContext _dbContext;

    public InterestService(TravelDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<InterestDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Interests
            .AsNoTracking()
            .OrderBy(i => i.Name)
            .Select(i => new InterestDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<InterestDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var interest = await _dbContext.Interests
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (interest is null)
        {
            throw new NotFoundException($"Interest with id {id} was not found.");
        }

        return Map(interest);
    }

    public async Task<InterestDto> CreateAsync(
        CreateInterestRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ContentValidators.ValidateInterest(request);
        await EnsureNameUniqueAsync(request.Name, excludeId: null, cancellationToken);

        var interest = new Interest
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim()
        };

        _dbContext.Interests.Add(interest);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(interest);
    }

    public async Task<InterestDto> UpdateAsync(
        int id,
        UpdateInterestRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ContentValidators.ValidateInterest(request);

        var interest = await _dbContext.Interests
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (interest is null)
        {
            throw new NotFoundException($"Interest with id {id} was not found.");
        }

        await EnsureNameUniqueAsync(request.Name, id, cancellationToken);

        interest.Name = request.Name.Trim();
        interest.Description = request.Description?.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(interest);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var interest = await _dbContext.Interests
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (interest is null)
        {
            throw new NotFoundException($"Interest with id {id} was not found.");
        }

        var usedInPreferences = await _dbContext.UserPreferenceInterests
            .AnyAsync(x => x.InterestId == id, cancellationToken);

        if (usedInPreferences)
        {
            throw new ConflictException(
                "Interest cannot be deleted because it is used in user preferences.");
        }

        var usedInDestinations = await _dbContext.DestinationInterests
            .AnyAsync(x => x.InterestId == id, cancellationToken);

        if (usedInDestinations)
        {
            throw new ConflictException(
                "This interest is currently assigned to one or more destinations and cannot be deleted.");
        }

        _dbContext.Interests.Remove(interest);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureNameUniqueAsync(string name, int? excludeId, CancellationToken cancellationToken)
    {
        var normalized = name.Trim();
        var exists = await _dbContext.Interests.AnyAsync(
            i => i.Name == normalized && (!excludeId.HasValue || i.Id != excludeId.Value),
            cancellationToken);

        if (exists)
        {
            throw new ConflictException($"Interest '{normalized}' already exists.");
        }
    }

    private static InterestDto Map(Interest interest)
    {
        return new InterestDto
        {
            Id = interest.Id,
            Name = interest.Name,
            Description = interest.Description
        };
    }
}
