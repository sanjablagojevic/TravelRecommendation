using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelRecommendation.Api.Extensions;
using TravelRecommendation.Application.DTOs.Ai;
using TravelRecommendation.Application.Interfaces;

namespace TravelRecommendation.Api.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiPreferenceService _aiPreferenceService;

    public AiController(IAiPreferenceService aiPreferenceService)
    {
        _aiPreferenceService = aiPreferenceService;
    }

    /// <summary>
    /// Extract structured preferences from natural language. Does not save preferences or recommend destinations.
    /// </summary>
    [HttpPost("extract-preferences")]
    public async Task<ActionResult<AiPreferenceExtractionResponseDto>> ExtractPreferences(
        [FromBody] AiPreferenceRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _aiPreferenceService.ExtractPreferencesAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a UserPreference from already confirmed structured data. Does not call OpenAI.
    /// </summary>
    [HttpPost("create-preference")]
    public async Task<ActionResult<CreatePreferenceFromAiResponseDto>> CreatePreference(
        [FromBody] CreatePreferenceFromAiRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _aiPreferenceService.CreatePreferenceAsync(userId, request, cancellationToken);
        return Ok(result);
    }
}
