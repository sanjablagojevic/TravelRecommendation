namespace TravelRecommendation.Application.DTOs.Categories;

public class CreateCategoryRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
}
