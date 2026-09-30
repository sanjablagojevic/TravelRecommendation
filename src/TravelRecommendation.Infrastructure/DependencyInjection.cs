using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelRecommendation.Application.Interfaces;
using TravelRecommendation.Application.Recommendations;
using TravelRecommendation.Application.Settings;
using TravelRecommendation.Infrastructure.ExternalServices.OpenAI;
using TravelRecommendation.Infrastructure.ExternalServices.Weather.OpenWeather;
using TravelRecommendation.Infrastructure.Identity;
using TravelRecommendation.Infrastructure.Persistence;
using TravelRecommendation.Infrastructure.Services;

namespace TravelRecommendation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<OpenAiSettings>(configuration.GetSection(OpenAiSettings.SectionName));
        services.Configure<WeatherSettings>(configuration.GetSection(WeatherSettings.SectionName));

        services.AddDbContext<TravelDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        var weights = new RecommendationWeights();
        weights.EnsureValid();
        services.AddSingleton(weights);
        services.AddSingleton<IRecommendationScoringService, RecommendationScoringService>();

        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDestinationService, DestinationService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IInterestService, InterestService>();
        services.AddScoped<IAttractionService, AttractionService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IUserPreferenceService, UserPreferenceService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<IRecommendationService, RecommendationService>();

        // OpenAI is registered always; missing API key only fails when AI endpoints are called.
        services.AddScoped<IOpenAiTextClient, OpenAiTextClient>();
        services.AddScoped<IAiPreferenceService, OpenAiPreferenceService>();
        services.AddScoped<IAiRecommendationExplanationService, OpenAiRecommendationExplanationService>();
        services.AddScoped<IAiItineraryService, OpenAiItineraryService>();
        services.AddScoped<IItineraryService, ItineraryService>();

        var weatherSettings = configuration.GetSection(WeatherSettings.SectionName).Get<WeatherSettings>()
                              ?? new WeatherSettings();
        var weatherTimeout = weatherSettings.TimeoutSeconds <= 0 ? 15 : weatherSettings.TimeoutSeconds;
        services.AddHttpClient(OpenWeatherService.HttpClientName, client =>
        {
            var baseUrl = string.IsNullOrWhiteSpace(weatherSettings.BaseUrl)
                ? "https://api.openweathermap.org"
                : weatherSettings.BaseUrl.TrimEnd('/');
            client.BaseAddress = new Uri(baseUrl + "/");
            client.Timeout = TimeSpan.FromSeconds(weatherTimeout);
        });
        services.AddScoped<IWeatherService, OpenWeatherService>();

        return services;
    }
}
