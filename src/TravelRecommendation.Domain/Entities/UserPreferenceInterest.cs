namespace TravelRecommendation.Domain.Entities;

public class UserPreferenceInterest
{
    public int UserPreferenceId { get; set; }
    public int InterestId { get; set; }

    public UserPreference UserPreference { get; set; } = null!;
    public Interest Interest { get; set; } = null!;
}
