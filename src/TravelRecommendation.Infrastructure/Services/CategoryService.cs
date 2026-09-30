using Microsoft.EntityFrameworkCore;
using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.DTOs.Categories;
using TravelRecommendation.Application.Interfaces;
using TravelRecommendation.Application.Validators;
using TravelRecommendation.Domain.Entities;
using TravelRecommendation.Infrastructure.Persistence;

namespace TravelRecommendation.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly TravelDbContext _dbContext;

    public CategoryService(TravelDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type,
                Description = c.Description
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category is null)
        {
            throw new NotFoundException($"Category with id {id} was not found.");
        }

        return Map(category);
    }

    public async Task<CategoryDto> CreateAsync(
        CreateCategoryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ContentValidators.ValidateCategory(request);
        await EnsureNameUniqueAsync(request.Name, excludeId: null, cancellationToken);

        var category = new Category
        {
            Name = request.Name.Trim(),
            Type = request.Type.Trim(),
            Description = request.Description?.Trim()
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(category);
    }

    public async Task<CategoryDto> UpdateAsync(
        int id,
        UpdateCategoryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ContentValidators.ValidateCategory(request);

        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category is null)
        {
            throw new NotFoundException($"Category with id {id} was not found.");
        }

        await EnsureNameUniqueAsync(request.Name, id, cancellationToken);

        category.Name = request.Name.Trim();
        category.Type = request.Type.Trim();
        category.Description = request.Description?.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(category);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category is null)
        {
            throw new NotFoundException($"Category with id {id} was not found.");
        }

        var isUsed = await _dbContext.DestinationCategories
            .AnyAsync(dc => dc.CategoryId == id, cancellationToken);

        if (isUsed)
        {
            throw new ConflictException(
                "This category is currently assigned to one or more destinations and cannot be deleted.");
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureNameUniqueAsync(string name, int? excludeId, CancellationToken cancellationToken)
    {
        var normalized = name.Trim();
        var exists = await _dbContext.Categories.AnyAsync(
            c => c.Name == normalized && (!excludeId.HasValue || c.Id != excludeId.Value),
            cancellationToken);

        if (exists)
        {
            throw new ConflictException($"Category '{normalized}' already exists.");
        }
    }

    private static CategoryDto Map(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type,
            Description = category.Description
        };
    }
}
