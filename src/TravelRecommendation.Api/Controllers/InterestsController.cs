using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelRecommendation.Application.DTOs.Interests;
using TravelRecommendation.Application.Interfaces;

namespace TravelRecommendation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InterestsController : ControllerBase
{
    private readonly IInterestService _interestService;

    public InterestsController(IInterestService interestService)
    {
        _interestService = interestService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<InterestDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _interestService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<InterestDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await _interestService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<InterestDto>> Create(
        [FromBody] CreateInterestRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _interestService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<InterestDto>> Update(
        int id,
        [FromBody] UpdateInterestRequestDto request,
        CancellationToken cancellationToken)
    {
        return Ok(await _interestService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _interestService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
