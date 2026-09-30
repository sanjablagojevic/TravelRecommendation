using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.DTOs.Weather;
using TravelRecommendation.Application.Interfaces;
using TravelRecommendation.Application.Settings;
using TravelRecommendation.Domain.Entities;
using TravelRecommendation.Infrastructure.Persistence;

namespace TravelRecommendation.Infrastructure.ExternalServices.Weather.OpenWeather;

public class OpenWeatherService : IWeatherService
{
    public const string HttpClientName = "OpenWeather";
    public const string CacheAction = "CurrentAndForecast";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TravelDbContext _dbContext;
    private readonly WeatherSettings _settings;
    private readonly ILogger<OpenWeatherService> _logger;

    public OpenWeatherService(
        IHttpClientFactory httpClientFactory,
        TravelDbContext dbContext,
        IOptions<WeatherSettings> settings,
        ILogger<OpenWeatherService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _dbContext = dbContext;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<WeatherResponseDto> GetDestinationWeatherAsync(
        int destinationId,
        CancellationToken cancellationToken = default)
    {
        var destination = await _dbContext.Destinations
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == destinationId, cancellationToken);

        if (destination is null || !destination.IsActive)
        {
            throw new NotFoundException("Destination was not found.");
        }

        if (!HasValidCoordinates(destination.Latitude, destination.Longitude))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["Coordinates"] = ["Destination does not have valid map coordinates for weather lookup."]
            });
        }

        var requestKey = BuildCacheKey(destinationId);
        var provider = string.IsNullOrWhiteSpace(_settings.Provider) ? "OpenWeather" : _settings.Provider.Trim();

        var cached = await _dbContext.ApiCaches
            .FirstOrDefaultAsync(
                c => c.Provider == provider
                     && c.Action == CacheAction
                     && c.RequestKey == requestKey,
                cancellationToken);

        var now = DateTime.UtcNow;
        if (cached is not null && cached.ExpiresAt > now)
        {
            _logger.LogInformation(
                "Weather cache hit for DestinationId={DestinationId}, Provider={Provider}",
                destinationId,
                provider);

            var fromCache = DeserializeCached(cached.ResponseJson, destinationId, destination.Name);
            fromCache.IsCached = true;
            fromCache.IsStale = false;
            return fromCache;
        }

        EnsureConfigured();

        try
        {
            var started = Stopwatch.StartNew();
            var fresh = await FetchFromProviderAsync(
                destinationId,
                destination.Name,
                destination.Latitude,
                destination.Longitude,
                cancellationToken);
            started.Stop();

            _logger.LogInformation(
                "Weather provider success for DestinationId={DestinationId}, Provider={Provider}, ElapsedMs={ElapsedMs}, CacheMiss={CacheMiss}",
                destinationId,
                provider,
                started.ElapsedMilliseconds,
                cached is null);

            await UpsertCacheAsync(cached, provider, requestKey, fresh, now, cancellationToken);
            fresh.IsCached = false;
            fresh.IsStale = false;
            return fresh;
        }
        catch (Exception ex) when (
            ex is not OperationCanceledException
            and not ValidationException
            and not NotFoundException
            and not ServiceUnavailableAppException)
        {
            _logger.LogWarning(
                ex,
                "Weather provider failure for DestinationId={DestinationId}, Provider={Provider}",
                destinationId,
                provider);

            throw new ServiceUnavailableAppException("Weather information is temporarily unavailable.");
        }
    }

    internal static string BuildCacheKey(int destinationId) =>
        $"weather:destination:{destinationId}";

    internal static bool HasValidCoordinates(decimal latitude, decimal longitude)
    {
        // Reject the common "missing coordinates" sentinel (0,0) and out-of-range values.
        if (latitude == 0m && longitude == 0m)
        {
            return false;
        }

        return latitude is >= -90m and <= 90m && longitude is >= -180m and <= 180m;
    }

    internal static WeatherResponseDto MapProviderResponses(
        int destinationId,
        string destinationName,
        OpenWeatherCurrentResponse current,
        OpenWeatherForecastResponse forecast,
        DateTime fetchedAtUtc)
    {
        var condition = current.Weather.FirstOrDefault();
        var main = current.Main ?? throw new InvalidOperationException("OpenWeather current response missing main.");

        var currentDto = new CurrentWeatherDto
        {
            Temperature = Round(main.Temp),
            FeelsLike = Round(main.FeelsLike),
            MinTemperature = Round(main.TempMin),
            MaxTemperature = Round(main.TempMax),
            Humidity = main.Humidity,
            Description = Capitalize(condition?.Description ?? string.Empty),
            Icon = BuildIconUrl(condition?.Icon),
            WindSpeed = Round(MetersPerSecondToKmPerHour(current.Wind?.Speed ?? 0m)),
            ObservedAt = DateTimeOffset.FromUnixTimeSeconds(current.Dt).UtcDateTime
        };

        var forecastDays = AggregateForecastDays(forecast.List);

        return new WeatherResponseDto
        {
            DestinationId = destinationId,
            DestinationName = destinationName,
            Current = currentDto,
            Forecast = forecastDays,
            FetchedAt = fetchedAtUtc,
            IsCached = false,
            IsStale = false
        };
    }

    internal static IReadOnlyList<ForecastDayDto> AggregateForecastDays(
        IEnumerable<OpenWeatherForecastItem> items)
    {
        return items
            .GroupBy(i => DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(i.Dt).UtcDateTime))
            .OrderBy(g => g.Key)
            .Take(5)
            .Select(g =>
            {
                var tempsMin = g.Select(x => x.Main?.TempMin ?? x.Main?.Temp ?? 0m).ToList();
                var tempsMax = g.Select(x => x.Main?.TempMax ?? x.Main?.Temp ?? 0m).ToList();
                var midday = g
                    .OrderBy(x => Math.Abs(DateTimeOffset.FromUnixTimeSeconds(x.Dt).UtcDateTime.Hour - 12))
                    .First();
                var condition = midday.Weather.FirstOrDefault();

                return new ForecastDayDto
                {
                    Date = g.Key,
                    MinTemperature = Round(tempsMin.Min()),
                    MaxTemperature = Round(tempsMax.Max()),
                    Description = Capitalize(condition?.Description ?? string.Empty),
                    Icon = BuildIconUrl(condition?.Icon)
                };
            })
            .ToList();
    }

    private async Task<WeatherResponseDto> FetchFromProviderAsync(
        int destinationId,
        string destinationName,
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);
        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lon = longitude.ToString(CultureInfo.InvariantCulture);
        var key = Uri.EscapeDataString(_settings.ApiKey);

        var currentUrl =
            $"data/2.5/weather?lat={lat}&lon={lon}&units=metric&appid={key}";
        var forecastUrl =
            $"data/2.5/forecast?lat={lat}&lon={lon}&units=metric&appid={key}";

        using var currentResponse = await client.GetAsync(currentUrl, cancellationToken);
        await EnsureSuccessAsync(currentResponse, cancellationToken);
        var current = await currentResponse.Content.ReadFromJsonAsync<OpenWeatherCurrentResponse>(
            JsonOptions,
            cancellationToken)
            ?? throw new ServiceUnavailableAppException("Weather information is temporarily unavailable.");

        using var forecastResponse = await client.GetAsync(forecastUrl, cancellationToken);
        await EnsureSuccessAsync(forecastResponse, cancellationToken);
        var forecast = await forecastResponse.Content.ReadFromJsonAsync<OpenWeatherForecastResponse>(
            JsonOptions,
            cancellationToken)
            ?? new OpenWeatherForecastResponse();

        return MapProviderResponses(destinationId, destinationName, current, forecast, DateTime.UtcNow);
    }

    private async Task UpsertCacheAsync(
        ApiCache? existing,
        string provider,
        string requestKey,
        WeatherResponseDto payload,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var ttlMinutes = _settings.CacheMinutes <= 0 ? 30 : _settings.CacheMinutes;
        var json = JsonSerializer.Serialize(payload, JsonOptions);

        if (existing is null)
        {
            _dbContext.ApiCaches.Add(new ApiCache
            {
                Provider = provider,
                Action = CacheAction,
                RequestKey = requestKey,
                ResponseJson = json,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(ttlMinutes)
            });
        }
        else
        {
            existing.ResponseJson = json;
            existing.CreatedAt = now;
            existing.ExpiresAt = now.AddMinutes(ttlMinutes);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private WeatherResponseDto DeserializeCached(string json, int destinationId, string destinationName)
    {
        var dto = JsonSerializer.Deserialize<WeatherResponseDto>(json, JsonOptions)
                  ?? throw new ServiceUnavailableAppException("Weather information is temporarily unavailable.");

        dto.DestinationId = destinationId;
        dto.DestinationName = destinationName;
        return dto;
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new ServiceUnavailableAppException(
                "Weather is not configured. Set Weather:ApiKey via user-secrets or environment variables.");
        }
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        // Drain body for diagnostics without logging secrets.
        _ = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new ServiceUnavailableAppException("Weather information is temporarily unavailable.");
        }

        throw new ServiceUnavailableAppException("Weather information is temporarily unavailable.");
    }

    private static decimal MetersPerSecondToKmPerHour(decimal metersPerSecond) =>
        metersPerSecond * 3.6m;

    private static decimal Round(decimal value) =>
        Math.Round(value, 1, MidpointRounding.AwayFromZero);

    private static string? BuildIconUrl(string? iconCode)
    {
        if (string.IsNullOrWhiteSpace(iconCode))
        {
            return null;
        }

        return $"https://openweathermap.org/img/wn/{iconCode}@2x.png";
    }

    private static string Capitalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return char.ToUpperInvariant(value[0]) + value[1..];
    }
}
