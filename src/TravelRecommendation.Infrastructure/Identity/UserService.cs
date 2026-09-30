using Microsoft.EntityFrameworkCore;
using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.DTOs.Users;
using TravelRecommendation.Application.Interfaces;
using TravelRecommendation.Application.Validators;
using TravelRecommendation.Infrastructure.Persistence;

namespace TravelRecommendation.Infrastructure.Identity;

public class UserService : IUserService
{
    private readonly TravelDbContext _dbContext;
    private readonly IPasswordService _passwordService;

    public UserService(TravelDbContext dbContext, IPasswordService passwordService)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
    }

    public async Task<UserProfileDto> GetProfileAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await GetActiveUserAsync(userId, cancellationToken);

        return new UserProfileDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.Name,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserProfileDto> UpdateProfileAsync(
        int userId,
        UpdateProfileRequestDto request,
        CancellationToken cancellationToken = default)
    {
        AuthValidators.ValidateUpdateProfile(request);

        var user = await GetActiveUserAsync(userId, cancellationToken);

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UserProfileDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.Name,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task ChangePasswordAsync(
        int userId,
        ChangePasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        AuthValidators.ValidateChangePassword(request);

        var user = await GetActiveUserAsync(userId, cancellationToken);

        if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAppException("Current password is incorrect.");
        }

        user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Domain.Entities.User> GetActiveUserAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User was not found.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAppException("User is not available.");
        }

        return user;
    }
}
