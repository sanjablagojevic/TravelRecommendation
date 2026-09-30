using TravelRecommendation.Application.DTOs.Weather;

namespace TravelRecommendation.Application.Interfaces;

public interface IWeatherService
{
    Task<WeatherResponseDto> GetDestinationWeatherAsync(
        int destinationId,
        CancellationToken cancellationToken = default);
}
