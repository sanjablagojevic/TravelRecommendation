using TravelRecommendation.Application.DTOs.Admin;

namespace TravelRecommendation.Application.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
}
