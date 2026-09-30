using TravelRecommendation.Domain.Common;

namespace TravelRecommendation.Domain.Entities;

public class Interest : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<UserPreferenceInterest> UserPreferenceInterests { get; set; } = new List<UserPreferenceInterest>();
    public ICollection<DestinationInterest> DestinationInterests { get; set; } = new List<DestinationInterest>();
}
