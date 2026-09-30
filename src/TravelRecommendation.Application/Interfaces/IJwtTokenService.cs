using TravelRecommendation.Domain.Entities;

namespace TravelRecommendation.Application.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
