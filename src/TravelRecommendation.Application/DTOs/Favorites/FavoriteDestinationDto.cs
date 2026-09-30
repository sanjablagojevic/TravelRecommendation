namespace TravelRecommendation.Application.DTOs.Favorites;

public class FavoriteDestinationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal AverageDailyCost { get; set; }
    public string? Climate { get; set; }
}
