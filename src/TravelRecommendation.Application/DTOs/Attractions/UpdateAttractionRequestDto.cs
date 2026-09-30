namespace TravelRecommendation.Application.DTOs.Attractions;

public class UpdateAttractionRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? ImageUrl { get; set; }
}
