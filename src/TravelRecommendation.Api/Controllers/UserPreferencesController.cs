using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelRecommendation.Api.Extensions;
using TravelRecommendation.Application.DTOs.UserPreferences;
using TravelRecommendation.Application.Interfaces;

namespace TravelRecommendation.Api.Controllers;

[ApiController]
[Route("api/preferences")]
[Authorize]
public class UserPreferencesController : ControllerBase
{
    private readonly IUserPreferenceService _userPreferenceService;

    public UserPreferencesController(IUserPreferenceService userPreferenceService)
    {
        _userPreferenceService = userPreferenceService;
    }

    [HttpPost]
    public async Task<ActionResult<UserPreferenceDetailsDto>> Create(
        [FromBody] CreateUserPreferenceRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _userPreferenceService.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserPreferenceDto>>> GetAll(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        return Ok(await _userPreferenceService.GetUserPreferencesAsync(userId, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserPreferenceDetailsDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        return Ok(await _userPreferenceService.GetByIdAsync(userId, id, cancellationToken));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserPreferenceDetailsDto>> Update(
        int id,
        [FromBody] UpdateUserPreferenceRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        return Ok(await _userPreferenceService.UpdateAsync(userId, id, request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await _userPreferenceService.DeleteAsync(userId, id, cancellationToken);
        return NoContent();
    }
}
