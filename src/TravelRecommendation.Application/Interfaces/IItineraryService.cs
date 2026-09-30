using TravelRecommendation.Application.Common;
using TravelRecommendation.Application.DTOs.Itineraries;

namespace TravelRecommendation.Application.Interfaces;

public interface IAiItineraryService
{
    Task<AiGeneratedItineraryDto> GenerateAsync(
        AiItineraryGenerationContext context,
        CancellationToken cancellationToken = default);
}

public interface IItineraryService
{
    Task<TravelItineraryDto> GenerateAsync(
        int userId,
        GenerateItineraryRequestDto request,
        CancellationToken cancellationToken = default);

    Task<TravelItineraryDto> GetByIdAsync(
        int userId,
        int id,
        CancellationToken cancellationToken = default);

    Task<PagedResult<ItineraryListDto>> GetUserItinerariesAsync(
        int userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int userId,
        int id,
        CancellationToken cancellationToken = default);
}
