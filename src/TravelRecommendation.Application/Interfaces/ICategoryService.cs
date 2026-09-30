using TravelRecommendation.Application.DTOs.Categories;

namespace TravelRecommendation.Application.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CategoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateAsync(CreateCategoryRequestDto request, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateAsync(int id, UpdateCategoryRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
