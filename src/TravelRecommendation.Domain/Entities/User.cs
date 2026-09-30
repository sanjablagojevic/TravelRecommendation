using TravelRecommendation.Domain.Common;

namespace TravelRecommendation.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public ICollection<UserPreference> UserPreferences { get; set; } = new List<UserPreference>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
    public ICollection<TravelHistory> TravelHistories { get; set; } = new List<TravelHistory>();
    public ICollection<ApiCache> ApiCaches { get; set; } = new List<ApiCache>();
    public ICollection<TravelItinerary> TravelItineraries { get; set; } = new List<TravelItinerary>();
}
