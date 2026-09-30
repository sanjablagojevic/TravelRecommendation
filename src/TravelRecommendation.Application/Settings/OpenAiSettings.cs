namespace TravelRecommendation.Application.Settings;

public class OpenAiSettings
{
    public const string SectionName = "OpenAI";

    /// <summary>Stored via user-secrets / environment variables. Never commit real keys.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Model id used for preference extraction and explanations. Configured in one place only.</summary>
    public string Model { get; set; } = "gpt-4o-mini";

    public int TimeoutSeconds { get; set; } = 60;
}
