using Microsoft.EntityFrameworkCore;
using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.DTOs.Attractions;
using TravelRecommendation.Application.Interfaces;
using TravelRecommendation.Application.Validators;
using TravelRecommendation.Domain.Entities;
using TravelRecommendation.Infrastructure.Persistence;

namespace TravelRecommendation.Infrastructure.Services;

public class AttractionService : IAttractionService
{
    private readonly TravelDbContext _dbContext;

    public AttractionService(TravelDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AttractionDto>> GetByDestinationAsync(
        int destinationId,
        CancellationToken cancellationToken = default)
    {
        await EnsureDestinationExistsAsync(destinationId, cancellationToken);

        return await _dbContext.Attractions
            .AsNoTracking()
            .Where(a => a.DestinationId == destinationId)
            .OrderBy(a => a.Name)
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
            .ToListAsync(cancellationToken);
    }

    public async Task<AttractionDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var attraction = await _dbContext.Attractions
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (attraction is null)
        {
            throw new NotFoundException($"Attraction with id {id} was not found.");
        }

        return Map(attraction);
    }

    public async Task<AttractionDto> CreateAsync(
        CreateAttractionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ContentValidators.ValidateAttractionCreate(request);
        await EnsureDestinationExistsAsync(request.DestinationId, cancellationToken);

        var attraction = new Attraction
        {
            DestinationId = request.DestinationId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Location = request.Location?.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            ImageUrl = request.ImageUrl?.Trim()
        };

        _dbContext.Attractions.Add(attraction);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(attraction);
    }

    public async Task<AttractionDto> UpdateAsync(
        int id,
        UpdateAttractionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ContentValidators.ValidateAttractionUpdate(request);

        var attraction = await _dbContext.Attractions
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (attraction is null)
        {
            throw new NotFoundException($"Attraction with id {id} was not found.");
        }

        attraction.Name = request.Name.Trim();
        attraction.Description = request.Description?.Trim();
        attraction.Location = request.Location?.Trim();
        attraction.Latitude = request.Latitude;
        attraction.Longitude = request.Longitude;
        attraction.ImageUrl = request.ImageUrl?.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(attraction);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var attraction = await _dbContext.Attractions
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (attraction is null)
        {
            throw new NotFoundException($"Attraction with id {id} was not found.");
        }

        _dbContext.Attractions.Remove(attraction);
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

    private static AttractionDto Map(Attraction attraction)
    {
        return new AttractionDto
        {
            Id = attraction.Id,
            DestinationId = attraction.DestinationId,
            Name = attraction.Name,
            Description = attraction.Description,
            Location = attraction.Location,
            Latitude = attraction.Latitude,
            Longitude = attraction.Longitude,
            ImageUrl = attraction.ImageUrl
        };
    }
}
