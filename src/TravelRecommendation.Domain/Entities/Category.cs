using TravelRecommendation.Domain.Common;

namespace TravelRecommendation.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<DestinationCategory> DestinationCategories { get; set; } = new List<DestinationCategory>();
}
