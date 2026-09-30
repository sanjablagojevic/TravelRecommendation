namespace TravelRecommendation.Application.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public CurrentUserDto User { get; set; } = null!;
}
