using TravelRecommendation.Application.DTOs.Activities;
using TravelRecommendation.Application.DTOs.Attractions;
using TravelRecommendation.Application.DTOs.Categories;

namespace TravelRecommendation.Application.DTOs.Destinations;

public class DestinationDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal AverageDailyCost { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal Popularity { get; set; }
    public string? ImageUrl { get; set; }
    public string? Climate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<CategoryDto> Categories { get; set; } = [];
    public IReadOnlyList<DestinationInterestDto> Interests { get; set; } = [];
    public IReadOnlyList<AttractionDto> Attractions { get; set; } = [];
    public IReadOnlyList<ActivityDto> Activities { get; set; } = [];
}
