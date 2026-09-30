using Microsoft.EntityFrameworkCore;
using TravelRecommendation.Application.DTOs.Admin;
using TravelRecommendation.Application.Interfaces;
using TravelRecommendation.Infrastructure.Persistence;

namespace TravelRecommendation.Infrastructure.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly TravelDbContext _dbContext;

    public AdminDashboardService(TravelDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var destinationsCount = await _dbContext.Destinations.CountAsync(cancellationToken);
        var activeDestinationsCount = await _dbContext.Destinations
            .CountAsync(d => d.IsActive, cancellationToken);
        var inactiveDestinationsCount = destinationsCount - activeDestinationsCount;

        return new AdminDashboardDto
        {
            DestinationsCount = destinationsCount,
            ActiveDestinationsCount = activeDestinationsCount,
            InactiveDestinationsCount = inactiveDestinationsCount,
            CategoriesCount = await _dbContext.Categories.CountAsync(cancellationToken),
            InterestsCount = await _dbContext.Interests.CountAsync(cancellationToken),
            AttractionsCount = await _dbContext.Attractions.CountAsync(cancellationToken),
            ActivitiesCount = await _dbContext.Activities.CountAsync(cancellationToken),
            UsersCount = await _dbContext.Users.CountAsync(cancellationToken),
            RecommendationsCount = await _dbContext.Recommendations.CountAsync(cancellationToken)
        };
    }
}
