using TravelRecommendation.Application.DTOs.Auth;

namespace TravelRecommendation.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<CurrentUserDto> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default);
}
