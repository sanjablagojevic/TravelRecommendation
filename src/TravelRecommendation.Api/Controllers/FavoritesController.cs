using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelRecommendation.Api.Extensions;
using TravelRecommendation.Application.DTOs.Favorites;
using TravelRecommendation.Application.Interfaces;

namespace TravelRecommendation.Api.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FavoriteDto>>> GetAll(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        return Ok(await _favoriteService.GetUserFavoritesAsync(userId, cancellationToken));
    }

    [HttpPost("{destinationId:int}")]
    public async Task<ActionResult<FavoriteDto>> Add(int destinationId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _favoriteService.AddAsync(userId, destinationId, cancellationToken);
        return CreatedAtAction(nameof(GetStatus), new { destinationId }, result);
    }

    [HttpDelete("{destinationId:int}")]
    public async Task<IActionResult> Remove(int destinationId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await _favoriteService.RemoveAsync(userId, destinationId, cancellationToken);
        return NoContent();
    }

    [HttpGet("{destinationId:int}/status")]
    public async Task<ActionResult<FavoriteStatusDto>> GetStatus(
        int destinationId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        return Ok(await _favoriteService.IsFavoriteAsync(userId, destinationId, cancellationToken));
    }
}
