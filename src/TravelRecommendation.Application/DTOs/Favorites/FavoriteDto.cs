namespace TravelRecommendation.Application.DTOs.Favorites;

public class FavoriteDto
{
    public int Id { get; set; }
    public DateTime AddedAt { get; set; }
    public FavoriteDestinationDto Destination { get; set; } = null!;
}
