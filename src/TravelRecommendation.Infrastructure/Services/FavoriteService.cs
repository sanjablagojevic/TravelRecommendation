using Microsoft.EntityFrameworkCore;
using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.DTOs.Favorites;
using TravelRecommendation.Application.Interfaces;
using TravelRecommendation.Domain.Entities;
using TravelRecommendation.Infrastructure.Persistence;

namespace TravelRecommendation.Infrastructure.Services;

public class FavoriteService : IFavoriteService
{
    private readonly TravelDbContext _dbContext;

    public FavoriteService(TravelDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<FavoriteDto>> GetUserFavoritesAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.AddedAt)
            .Select(f => new FavoriteDto
            {
                Id = f.Id,
                AddedAt = f.AddedAt,
                Destination = new FavoriteDestinationDto
                {
                    Id = f.Destination.Id,
                    Name = f.Destination.Name,
                    Country = f.Destination.Country,
                    City = f.Destination.City,
                    ImageUrl = f.Destination.ImageUrl,
                    AverageDailyCost = f.Destination.AverageDailyCost,
                    Climate = f.Destination.Climate
                }
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<FavoriteDto> AddAsync(
        int userId,
        int destinationId,
        CancellationToken cancellationToken = default)
    {
        var destination = await _dbContext.Destinations
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == destinationId, cancellationToken);

        if (destination is null || !destination.IsActive)
        {
            throw new NotFoundException($"Active destination with id {destinationId} was not found.");
        }

        var alreadyExists = await _dbContext.Favorites
            .AnyAsync(f => f.UserId == userId && f.DestinationId == destinationId, cancellationToken);

        if (alreadyExists)
        {
            throw new ConflictException("Destination is already in favorites.");
        }

        var favorite = new Favorite
        {
            UserId = userId,
            DestinationId = destinationId,
            AddedAt = DateTime.UtcNow
        };

        _dbContext.Favorites.Add(favorite);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new FavoriteDto
        {
            Id = favorite.Id,
            AddedAt = favorite.AddedAt,
            Destination = new FavoriteDestinationDto
            {
                Id = destination.Id,
                Name = destination.Name,
                Country = destination.Country,
                City = destination.City,
                ImageUrl = destination.ImageUrl,
                AverageDailyCost = destination.AverageDailyCost,
                Climate = destination.Climate
            }
        };
    }

    public async Task RemoveAsync(
        int userId,
        int destinationId,
        CancellationToken cancellationToken = default)
    {
        var favorite = await _dbContext.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.DestinationId == destinationId, cancellationToken);

        if (favorite is null)
        {
            throw new NotFoundException("Favorite was not found.");
        }

        _dbContext.Favorites.Remove(favorite);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<FavoriteStatusDto> IsFavoriteAsync(
        int userId,
        int destinationId,
        CancellationToken cancellationToken = default)
    {
        var isFavorite = await _dbContext.Favorites
            .AsNoTracking()
            .AnyAsync(f => f.UserId == userId && f.DestinationId == destinationId, cancellationToken);

        return new FavoriteStatusDto { IsFavorite = isFavorite };
    }
}
