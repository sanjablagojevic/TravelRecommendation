using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelRecommendation.Api.Extensions;
using TravelRecommendation.Application.Common;
using TravelRecommendation.Application.DTOs.Itineraries;
using TravelRecommendation.Application.Interfaces;

namespace TravelRecommendation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItinerariesController : ControllerBase
{
    private readonly IItineraryService _itineraryService;

    public ItinerariesController(IItineraryService itineraryService)
    {
        _itineraryService = itineraryService;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<TravelItineraryDto>> Generate(
        [FromBody] GenerateItineraryRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _itineraryService.GenerateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ItineraryListDto>>> GetMine(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var result = await _itineraryService.GetUserItinerariesAsync(userId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TravelItineraryDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _itineraryService.GetByIdAsync(userId, id, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await _itineraryService.DeleteAsync(userId, id, cancellationToken);
        return NoContent();
    }
}
