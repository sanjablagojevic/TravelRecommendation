using Microsoft.EntityFrameworkCore;
using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.DTOs.Auth;
using TravelRecommendation.Application.Interfaces;
using TravelRecommendation.Application.Validators;
using TravelRecommendation.Domain.Entities;
using TravelRecommendation.Infrastructure.Persistence;

namespace TravelRecommendation.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly TravelDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        TravelDbContext dbContext,
        IPasswordService passwordService,
        IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default)
    {
        AuthValidators.ValidateRegister(request);

        var normalizedEmail = AuthValidators.NormalizeEmail(request.Email);

        var emailExists = await _dbContext.Users
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            throw new ConflictException("A user with this email already exists.");
        }

        var userRole = await _dbContext.Roles
            .FirstOrDefaultAsync(r => r.Name == "User", cancellationToken)
            ?? throw new NotFoundException("Default role 'User' was not found.");

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordService.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            RoleId = userRole.Id,
            Role = userRole
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        AuthValidators.ValidateLogin(request);

        var normalizedEmail = AuthValidators.NormalizeEmail(request.Email);

        var user = await _dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        return CreateAuthResponse(user);
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAppException("User is not available.");
        }

        return MapToCurrentUser(user);
    }

    private AuthResponseDto CreateAuthResponse(User user)
    {
        var (token, expiresAt) = _jwtTokenService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = MapToCurrentUser(user)
        };
    }

    private static CurrentUserDto MapToCurrentUser(User user)
    {
        return new CurrentUserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.Name
        };
    }
}
