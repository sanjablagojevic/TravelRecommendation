namespace TravelRecommendation.Application.DTOs.Destinations;

public class UpdateDestinationRequestDto
{
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
    public bool IsActive { get; set; } = true;
    public List<int> CategoryIds { get; set; } = [];
    public List<int> InterestIds { get; set; } = [];
}
