using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelRecommendation.Application.DTOs.Admin;
using TravelRecommendation.Application.Interfaces;

namespace TravelRecommendation.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminDashboardService _dashboardService;

    public AdminController(IAdminDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardDto>> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetDashboardAsync(cancellationToken);
        return Ok(result);
    }
}
