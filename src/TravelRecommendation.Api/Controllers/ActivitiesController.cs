using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelRecommendation.Application.DTOs.Activities;
using TravelRecommendation.Application.Interfaces;

namespace TravelRecommendation.Api.Controllers;

[ApiController]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ActivitiesController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    [HttpGet("api/destinations/{destinationId:int}/activities")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ActivityDto>>> GetByDestination(
        int destinationId,
        CancellationToken cancellationToken)
    {
        return Ok(await _activityService.GetByDestinationAsync(destinationId, cancellationToken));
    }

    [HttpGet("api/activities/{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ActivityDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await _activityService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost("api/activities")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ActivityDto>> Create(
        [FromBody] CreateActivityRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _activityService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("api/activities/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ActivityDto>> Update(
        int id,
        [FromBody] UpdateActivityRequestDto request,
        CancellationToken cancellationToken)
    {
        return Ok(await _activityService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("api/activities/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _activityService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
