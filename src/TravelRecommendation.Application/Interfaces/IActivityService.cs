using TravelRecommendation.Application.DTOs.Activities;

namespace TravelRecommendation.Application.Interfaces;

public interface IActivityService
{
    Task<IReadOnlyList<ActivityDto>> GetByDestinationAsync(int destinationId, CancellationToken cancellationToken = default);
    Task<ActivityDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ActivityDto> CreateAsync(CreateActivityRequestDto request, CancellationToken cancellationToken = default);
    Task<ActivityDto> UpdateAsync(int id, UpdateActivityRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
