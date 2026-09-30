using TravelRecommendation.Application.DTOs.Attractions;

namespace TravelRecommendation.Application.Interfaces;

public interface IAttractionService
{
    Task<IReadOnlyList<AttractionDto>> GetByDestinationAsync(int destinationId, CancellationToken cancellationToken = default);
    Task<AttractionDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AttractionDto> CreateAsync(CreateAttractionRequestDto request, CancellationToken cancellationToken = default);
    Task<AttractionDto> UpdateAsync(int id, UpdateAttractionRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
