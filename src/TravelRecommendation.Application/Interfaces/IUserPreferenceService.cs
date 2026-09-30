using TravelRecommendation.Application.DTOs.UserPreferences;

namespace TravelRecommendation.Application.Interfaces;

public interface IUserPreferenceService
{
    Task<UserPreferenceDetailsDto> CreateAsync(
        int userId,
        CreateUserPreferenceRequestDto request,
        CancellationToken cancellationToken = default);

    Task<UserPreferenceDetailsDto> GetByIdAsync(
        int userId,
        int preferenceId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserPreferenceDto>> GetUserPreferencesAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<UserPreferenceDetailsDto> UpdateAsync(
        int userId,
        int preferenceId,
        UpdateUserPreferenceRequestDto request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int userId,
        int preferenceId,
        CancellationToken cancellationToken = default);
}
