using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelRecommendation.Api.Extensions;
using TravelRecommendation.Application.Common;
using TravelRecommendation.Application.DTOs.Ai;
using TravelRecommendation.Application.DTOs.Recommendations;
using TravelRecommendation.Application.Interfaces;

namespace TravelRecommendation.Api.Controllers;

[ApiController]
[Route("api/recommendations")]
[Authorize]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;
    private readonly IAiRecommendationExplanationService _aiExplanationService;

    public RecommendationsController(
        IRecommendationService recommendationService,
        IAiRecommendationExplanationService aiExplanationService)
    {
        _recommendationService = recommendationService;
        _aiExplanationService = aiExplanationService;
    }

    [HttpPost]
    public async Task<ActionResult<RecommendationDto>> Generate(
        [FromBody] GenerateRecommendationRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _recommendationService.GenerateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<RecommendationHistoryDto>>> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var result = await _recommendationService.GetHistoryAsync(userId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RecommendationDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _recommendationService.GetByIdAsync(userId, id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Optionally generate AI explanations for an existing recommendation.
    /// Does not change Score, Rank, or EstimatedCost. Falls back to deterministic explanations on OpenAI failure.
    /// </summary>
    [HttpPost("{id:int}/ai-explanations")]
    public async Task<ActionResult<AiExplanationsResponseDto>> GenerateAiExplanations(
        int id,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _aiExplanationService.GenerateExplanationsAsync(userId, id, cancellationToken);
        return Ok(result);
    }
}
