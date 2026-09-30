using TravelRecommendation.Application.DTOs.Ai;

namespace TravelRecommendation.Application.Interfaces;

public interface IAiPreferenceService
{
    Task<AiPreferenceExtractionResponseDto> ExtractPreferencesAsync(
        AiPreferenceRequestDto request,
        CancellationToken cancellationToken = default);

    Task<CreatePreferenceFromAiResponseDto> CreatePreferenceAsync(
        int userId,
        CreatePreferenceFromAiRequestDto request,
        CancellationToken cancellationToken = default);
}
