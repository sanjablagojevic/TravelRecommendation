using TravelRecommendation.Domain.Common;

namespace TravelRecommendation.Domain.Entities;

public class ApiCache : BaseEntity
{
    public int? UserId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string RequestKey { get; set; } = string.Empty;
    public string ResponseJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public User? User { get; set; }
}
