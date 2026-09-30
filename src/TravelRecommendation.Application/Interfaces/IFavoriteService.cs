using TravelRecommendation.Application.DTOs.Favorites;

namespace TravelRecommendation.Application.Interfaces;

public interface IFavoriteService
{
    Task<IReadOnlyList<FavoriteDto>> GetUserFavoritesAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<FavoriteDto> AddAsync(
        int userId,
        int destinationId,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        int userId,
        int destinationId,
        CancellationToken cancellationToken = default);

    Task<FavoriteStatusDto> IsFavoriteAsync(
        int userId,
        int destinationId,
        CancellationToken cancellationToken = default);
}
