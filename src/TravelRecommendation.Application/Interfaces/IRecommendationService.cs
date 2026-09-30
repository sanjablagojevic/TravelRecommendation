using TravelRecommendation.Application.Common;
using TravelRecommendation.Application.DTOs.Recommendations;

namespace TravelRecommendation.Application.Interfaces;

public interface IRecommendationService
{
    Task<RecommendationDto> GenerateAsync(
        int userId,
        GenerateRecommendationRequestDto request,
        CancellationToken cancellationToken = default);

    Task<RecommendationDto> GetByIdAsync(
        int userId,
        int recommendationId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<RecommendationHistoryDto>> GetHistoryAsync(
        int userId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
}
