using TravelRecommendation.Application.DTOs.Categories;

namespace TravelRecommendation.Application.DTOs.Destinations;

public class DestinationListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal AverageDailyCost { get; set; }
    public decimal Popularity { get; set; }
    public string? ImageUrl { get; set; }
    public string? Climate { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<CategoryDto> Categories { get; set; } = [];
    public IReadOnlyList<DestinationInterestDto> Interests { get; set; } = [];
}
