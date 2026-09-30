using System.ClientModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Responses;
using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.Interfaces;
using TravelRecommendation.Application.Settings;

namespace TravelRecommendation.Infrastructure.ExternalServices.OpenAI;

/// <summary>
/// Official OpenAI .NET SDK Responses API + Structured Outputs.
/// OPENAI001 is suppressed at project level because Responses types are experimental in SDK 2.8.0.
/// </summary>
public class OpenAiTextClient : IOpenAiTextClient
{
    private readonly OpenAiSettings _settings;
    private readonly OpenAIClientOptions _clientOptions;
    private readonly ILogger<OpenAiTextClient> _logger;

    public OpenAiTextClient(
        IOptions<OpenAiSettings> settings,
        ILogger<OpenAiTextClient> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _clientOptions = new OpenAIClientOptions
        {
            NetworkTimeout = TimeSpan.FromSeconds(
                _settings.TimeoutSeconds <= 0 ? 60 : _settings.TimeoutSeconds)
        };
    }

    public async Task<string> CompleteJsonAsync(
        string instructions,
        string userInput,
        string jsonSchemaName,
        string jsonSchema,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        try
        {
            var client = CreateResponsesClient();
            var options = new CreateResponseOptions
            {
                Instructions = instructions,
                TextOptions = new ResponseTextOptions
                {
                    TextFormat = ResponseTextFormat.CreateJsonSchemaFormat(
                        jsonSchemaFormatName: jsonSchemaName,
                        jsonSchema: BinaryData.FromString(jsonSchema),
                        jsonSchemaIsStrict: true)
                }
            };
            options.InputItems.Add(ResponseItem.CreateUserMessageItem(userInput));

            var started = DateTime.UtcNow;
            var response = await client.CreateResponseAsync(options, cancellationToken);
            _logger.LogInformation(
                "OpenAI structured response completed in {ElapsedMs} ms using model configuration.",
                (DateTime.UtcNow - started).TotalMilliseconds);

            var text = response.Value.GetOutputText();
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ServiceUnavailableAppException("OpenAI returned an empty structured response.");
            }

            return text;
        }
        catch (ServiceUnavailableAppException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OpenAI structured request failed.");
            throw new ServiceUnavailableAppException("OpenAI preference extraction is temporarily unavailable.");
        }
    }

    public async Task<string> CompleteTextAsync(
        string instructions,
        string userInput,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        try
        {
            var client = CreateResponsesClient();
            var options = new CreateResponseOptions
            {
                Instructions = instructions
            };
            options.InputItems.Add(ResponseItem.CreateUserMessageItem(userInput));

            var started = DateTime.UtcNow;
            var response = await client.CreateResponseAsync(options, cancellationToken);
            _logger.LogInformation(
                "OpenAI text response completed in {ElapsedMs} ms.",
                (DateTime.UtcNow - started).TotalMilliseconds);

            var text = response.Value.GetOutputText();
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ServiceUnavailableAppException("OpenAI returned an empty explanation.");
            }

            return text.Trim();
        }
        catch (ServiceUnavailableAppException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OpenAI text request failed.");
            throw new ServiceUnavailableAppException("OpenAI explanation service is temporarily unavailable.");
        }
    }

    private ResponsesClient CreateResponsesClient()
    {
        return new ResponsesClient(
            _settings.Model,
            new ApiKeyCredential(_settings.ApiKey),
            _clientOptions);
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new ServiceUnavailableAppException(
                "OpenAI is not configured. Set OpenAI:ApiKey via user-secrets or environment variables.");
        }

        if (string.IsNullOrWhiteSpace(_settings.Model))
        {
            throw new InvalidOperationException(
                "OpenAI:Model is not configured. Set it in appsettings or user-secrets.");
        }
    }
}
