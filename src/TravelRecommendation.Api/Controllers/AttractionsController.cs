using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelRecommendation.Application.DTOs.Attractions;
using TravelRecommendation.Application.Interfaces;

namespace TravelRecommendation.Api.Controllers;

[ApiController]
public class AttractionsController : ControllerBase
{
    private readonly IAttractionService _attractionService;

    public AttractionsController(IAttractionService attractionService)
    {
        _attractionService = attractionService;
    }

    [HttpGet("api/destinations/{destinationId:int}/attractions")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<AttractionDto>>> GetByDestination(
        int destinationId,
        CancellationToken cancellationToken)
    {
        return Ok(await _attractionService.GetByDestinationAsync(destinationId, cancellationToken));
    }

    [HttpGet("api/attractions/{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<AttractionDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await _attractionService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost("api/attractions")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AttractionDto>> Create(
        [FromBody] CreateAttractionRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _attractionService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("api/attractions/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AttractionDto>> Update(
        int id,
        [FromBody] UpdateAttractionRequestDto request,
        CancellationToken cancellationToken)
    {
        return Ok(await _attractionService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("api/attractions/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _attractionService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
