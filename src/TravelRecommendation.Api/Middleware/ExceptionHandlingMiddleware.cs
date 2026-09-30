using System.Net;
using System.Text.Json;
using TravelRecommendation.Application.Common.Exceptions;

namespace TravelRecommendation.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = (int)HttpStatusCode.InternalServerError;
        object body;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = validationException.StatusCode;
                body = new
                {
                    message = validationException.Message,
                    errors = validationException.Errors
                };
                break;

            case AppException appException:
                statusCode = appException.StatusCode;
                body = new { message = appException.Message };
                break;

            case UnauthorizedAccessException unauthorizedAccessException:
                statusCode = (int)HttpStatusCode.Unauthorized;
                body = new { message = unauthorizedAccessException.Message };
                break;

            default:
                _logger.LogError(exception, "Unhandled exception");
                body = _environment.IsDevelopment()
                    ? new { message = exception.Message, detail = exception.ToString() }
                    : new { message = "An unexpected error occurred." };
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }
}
