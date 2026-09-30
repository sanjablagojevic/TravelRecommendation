namespace TravelRecommendation.Application.DTOs.Destinations;

public class DestinationQueryParameters
{
    public string? Search { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public int? CategoryId { get; set; }
    public int? InterestId { get; set; }
    public bool? IsActive { get; set; }

    /// <summary>
    /// When true and <see cref="IsActive"/> is null, returns both active and inactive destinations.
    /// Non-admin callers are forced back to active-only by the API controller.
    /// </summary>
    public bool IncludeInactive { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
