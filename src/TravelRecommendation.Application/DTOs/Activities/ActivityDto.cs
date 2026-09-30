namespace TravelRecommendation.Application.DTOs.Activities;

public class ActivityDto
{
    public int Id { get; set; }
    public int DestinationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Provider { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public string? ExternalUrl { get; set; }
    public string? Description { get; set; }
}
