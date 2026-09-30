using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelRecommendation.Application.Common;
using TravelRecommendation.Application.DTOs.Destinations;
using TravelRecommendation.Application.DTOs.Weather;
using TravelRecommendation.Application.Interfaces;

namespace TravelRecommendation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DestinationsController : ControllerBase
{
    private readonly IDestinationService _destinationService;
    private readonly IWeatherService _weatherService;

    public DestinationsController(
        IDestinationService destinationService,
        IWeatherService weatherService)
    {
        _destinationService = destinationService;
        _weatherService = weatherService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<DestinationListDto>>> GetAll(
        [FromQuery] DestinationQueryParameters query,
        CancellationToken cancellationToken)
    {
        // Inactive / all listings are admin-only; public callers always see active destinations.
        if (!User.IsInRole("Admin"))
        {
            query.IncludeInactive = false;
            if (query.IsActive != true)
            {
                query.IsActive = true;
            }
        }

        var result = await _destinationService.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<DestinationDetailsDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _destinationService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Current weather and short-range forecast for a destination.
    /// Does not affect recommendation scoring.
    /// </summary>
    [HttpGet("{id:int}/weather")]
    [AllowAnonymous]
    public async Task<ActionResult<WeatherResponseDto>> GetWeather(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _weatherService.GetDestinationWeatherAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DestinationDetailsDto>> Create(
        [FromBody] CreateDestinationRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _destinationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DestinationDetailsDto>> Update(
        int id,
        [FromBody] UpdateDestinationRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _destinationService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _destinationService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
