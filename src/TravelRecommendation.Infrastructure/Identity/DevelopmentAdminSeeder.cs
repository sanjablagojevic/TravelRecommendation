using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TravelRecommendation.Application.Interfaces;
using TravelRecommendation.Domain.Entities;
using TravelRecommendation.Infrastructure.Persistence;

namespace TravelRecommendation.Infrastructure.Identity;

public class DevelopmentAdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DevelopmentAdminSeeder>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TravelDbContext>();
        var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();

        var email = configuration["SeedAdmin:Email"];
        var password = configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "SeedAdmin:Email or SeedAdmin:Password is not configured. Skipping admin seed.");
            return;
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();

        var adminRole = await dbContext.Roles
            .FirstOrDefaultAsync(r => r.Name == "Admin");

        if (adminRole is null)
        {
            logger.LogWarning("Admin role was not found. Skipping admin seed.");
            return;
        }

        var existingAdmin = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (existingAdmin is not null)
        {
            return;
        }

        var adminUser = new User
        {
            FirstName = "System",
            LastName = "Admin",
            Email = normalizedEmail,
            PasswordHash = passwordService.HashPassword(password),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            RoleId = adminRole.Id
        };

        dbContext.Users.Add(adminUser);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Development admin user seeded: {Email}", normalizedEmail);
    }
}
