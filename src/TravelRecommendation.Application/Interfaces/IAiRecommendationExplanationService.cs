using TravelRecommendation.Application.DTOs.Ai;

namespace TravelRecommendation.Application.Interfaces;

public interface IAiRecommendationExplanationService
{
    Task<AiExplanationsResponseDto> GenerateExplanationsAsync(
        int userId,
        int recommendationId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Low-level OpenAI text generation used by explanation orchestration.
/// Allows unit tests to mock OpenAI without network calls.
/// </summary>
public interface IOpenAiTextClient
{
    Task<string> CompleteJsonAsync(
        string instructions,
        string userInput,
        string jsonSchemaName,
        string jsonSchema,
        CancellationToken cancellationToken = default);

    Task<string> CompleteTextAsync(
        string instructions,
        string userInput,
        CancellationToken cancellationToken = default);
}
