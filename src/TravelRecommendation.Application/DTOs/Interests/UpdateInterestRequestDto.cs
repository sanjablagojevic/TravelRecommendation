namespace TravelRecommendation.Application.DTOs.Interests;

public class UpdateInterestRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
