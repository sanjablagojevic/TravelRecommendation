using TravelRecommendation.Application.DTOs.Users;

namespace TravelRecommendation.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileRequestDto request, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default);
}
