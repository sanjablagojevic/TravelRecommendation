using TravelRecommendation.Application.Common;
using TravelRecommendation.Application.DTOs.Destinations;

namespace TravelRecommendation.Application.Interfaces;

public interface IDestinationService
{
    Task<PagedResult<DestinationListDto>> GetAllAsync(
        DestinationQueryParameters query,
        CancellationToken cancellationToken = default);

    Task<DestinationDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<DestinationDetailsDto> CreateAsync(
        CreateDestinationRequestDto request,
        CancellationToken cancellationToken = default);

    Task<DestinationDetailsDto> UpdateAsync(
        int id,
        UpdateDestinationRequestDto request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
