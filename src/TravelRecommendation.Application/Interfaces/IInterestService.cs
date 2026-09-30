using TravelRecommendation.Application.DTOs.Interests;

namespace TravelRecommendation.Application.Interfaces;

public interface IInterestService
{
    Task<IReadOnlyList<InterestDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InterestDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<InterestDto> CreateAsync(CreateInterestRequestDto request, CancellationToken cancellationToken = default);
    Task<InterestDto> UpdateAsync(int id, UpdateInterestRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
