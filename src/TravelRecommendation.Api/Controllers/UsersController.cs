using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelRecommendation.Api.Extensions;
using TravelRecommendation.Application.DTOs.Users;
using TravelRecommendation.Application.Interfaces;

namespace TravelRecommendation.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetMe(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var profile = await _userService.GetProfileAsync(userId, cancellationToken);
        return Ok(profile);
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> UpdateMe(
        [FromBody] UpdateProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var profile = await _userService.UpdateProfileAsync(userId, request, cancellationToken);
        return Ok(profile);
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await _userService.ChangePasswordAsync(userId, request, cancellationToken);
        return NoContent();
    }
}
