using TravelRecommendation.Application.Common.Exceptions;
using TravelRecommendation.Application.DTOs.Activities;
using TravelRecommendation.Application.DTOs.Attractions;
using TravelRecommendation.Application.DTOs.Categories;
using TravelRecommendation.Application.DTOs.Destinations;
using TravelRecommendation.Application.DTOs.Interests;

namespace TravelRecommendation.Application.Validators;

public static class ContentValidators
{
    public static void ValidateDestinationCreate(CreateDestinationRequestDto request)
    {
        ValidateDestinationFields(
            request.Name,
            request.Country,
            request.City,
            request.AverageDailyCost,
            request.Latitude,
            request.Longitude,
            request.Popularity,
            request.ImageUrl,
            request.Climate);
    }

    public static void ValidateDestinationUpdate(UpdateDestinationRequestDto request)
    {
        ValidateDestinationFields(
            request.Name,
            request.Country,
            request.City,
            request.AverageDailyCost,
            request.Latitude,
            request.Longitude,
            request.Popularity,
            request.ImageUrl,
            request.Climate);
    }

    public static void ValidateCategory(CreateCategoryRequestDto request)
    {
        ValidateCategoryFields(request.Name, request.Type, request.Description);
    }

    public static void ValidateCategory(UpdateCategoryRequestDto request)
    {
        ValidateCategoryFields(request.Name, request.Type, request.Description);
    }

    public static void ValidateInterest(CreateInterestRequestDto request)
    {
        ValidateInterestFields(request.Name, request.Description);
    }

    public static void ValidateInterest(UpdateInterestRequestDto request)
    {
        ValidateInterestFields(request.Name, request.Description);
    }

    public static void ValidateAttractionCreate(CreateAttractionRequestDto request)
    {
        ValidateAttractionFields(
            request.Name,
            request.Location,
            request.Latitude,
            request.Longitude,
            request.ImageUrl,
            requireDestinationId: true,
            destinationId: request.DestinationId);
    }

    public static void ValidateAttractionUpdate(UpdateAttractionRequestDto request)
    {
        ValidateAttractionFields(
            request.Name,
            request.Location,
            request.Latitude,
            request.Longitude,
            request.ImageUrl,
            requireDestinationId: false,
            destinationId: null);
    }

    public static void ValidateActivityCreate(CreateActivityRequestDto request)
    {
        ValidateActivityFields(
            request.Name,
            request.Price,
            request.Currency,
            request.ExternalUrl,
            requireDestinationId: true,
            destinationId: request.DestinationId);
    }

    public static void ValidateActivityUpdate(UpdateActivityRequestDto request)
    {
        ValidateActivityFields(
            request.Name,
            request.Price,
            request.Currency,
            request.ExternalUrl,
            requireDestinationId: false,
            destinationId: null);
    }

    public static void NormalizePaging(DestinationQueryParameters query)
    {
        if (query.Page < 1)
        {
            query.Page = 1;
        }

        if (query.PageSize < 1)
        {
            query.PageSize = 10;
        }

        if (query.PageSize > 100)
        {
            query.PageSize = 100;
        }
    }

    private static void ValidateDestinationFields(
        string name,
        string country,
        string city,
        decimal averageDailyCost,
        decimal latitude,
        decimal longitude,
        decimal popularity,
        string? imageUrl,
        string? climate)
    {
        var errors = new Dictionary<string, List<string>>();

        RequireMaxLength(errors, "Name", name, 150, required: true);
        RequireMaxLength(errors, "Country", country, 100, required: true);
        RequireMaxLength(errors, "City", city, 100, required: true);

        if (averageDailyCost < 0)
        {
            Add(errors, "AverageDailyCost", "AverageDailyCost must be greater than or equal to 0.");
        }

        if (latitude < -90 || latitude > 90)
        {
            Add(errors, "Latitude", "Latitude must be between -90 and 90.");
        }

        if (longitude < -180 || longitude > 180)
        {
            Add(errors, "Longitude", "Longitude must be between -180 and 180.");
        }

        if (popularity < 0 || popularity > 1)
        {
            Add(errors, "Popularity", "Popularity must be between 0 and 1.");
        }

        if (!string.IsNullOrWhiteSpace(imageUrl) && imageUrl.Length > 1000)
        {
            Add(errors, "ImageUrl", "ImageUrl must be at most 1000 characters.");
        }

        if (!string.IsNullOrWhiteSpace(climate) && climate.Length > 100)
        {
            Add(errors, "Climate", "Climate must be at most 100 characters.");
        }

        ThrowIfErrors(errors);
    }

    private static void ValidateCategoryFields(string name, string type, string? description)
    {
        var errors = new Dictionary<string, List<string>>();
        RequireMaxLength(errors, "Name", name, 150, required: true);
        RequireMaxLength(errors, "Type", type, 100, required: true);

        if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
        {
            Add(errors, "Description", "Description must be at most 500 characters.");
        }

        ThrowIfErrors(errors);
    }

    private static void ValidateInterestFields(string name, string? description)
    {
        var errors = new Dictionary<string, List<string>>();
        RequireMaxLength(errors, "Name", name, 150, required: true);

        if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
        {
            Add(errors, "Description", "Description must be at most 500 characters.");
        }

        ThrowIfErrors(errors);
    }

    private static void ValidateAttractionFields(
        string name,
        string? location,
        decimal? latitude,
        decimal? longitude,
        string? imageUrl,
        bool requireDestinationId,
        int? destinationId)
    {
        var errors = new Dictionary<string, List<string>>();
        RequireMaxLength(errors, "Name", name, 150, required: true);

        if (requireDestinationId && (destinationId is null or <= 0))
        {
            Add(errors, "DestinationId", "DestinationId is required.");
        }

        if (!string.IsNullOrWhiteSpace(location) && location.Length > 250)
        {
            Add(errors, "Location", "Location must be at most 250 characters.");
        }

        if (latitude is < -90 or > 90)
        {
            Add(errors, "Latitude", "Latitude must be between -90 and 90.");
        }

        if (longitude is < -180 or > 180)
        {
            Add(errors, "Longitude", "Longitude must be between -180 and 180.");
        }

        if (!string.IsNullOrWhiteSpace(imageUrl) && imageUrl.Length > 1000)
        {
            Add(errors, "ImageUrl", "ImageUrl must be at most 1000 characters.");
        }

        ThrowIfErrors(errors);
    }

    private static void ValidateActivityFields(
        string name,
        decimal? price,
        string? currency,
        string? externalUrl,
        bool requireDestinationId,
        int? destinationId)
    {
        var errors = new Dictionary<string, List<string>>();
        RequireMaxLength(errors, "Name", name, 150, required: true);

        if (requireDestinationId && (destinationId is null or <= 0))
        {
            Add(errors, "DestinationId", "DestinationId is required.");
        }

        if (price is < 0)
        {
            Add(errors, "Price", "Price must be greater than or equal to 0.");
        }

        if (!string.IsNullOrWhiteSpace(currency) && currency.Length > 10)
        {
            Add(errors, "Currency", "Currency must be at most 10 characters.");
        }

        if (!string.IsNullOrWhiteSpace(externalUrl) && externalUrl.Length > 1000)
        {
            Add(errors, "ExternalUrl", "ExternalUrl must be at most 1000 characters.");
        }

        ThrowIfErrors(errors);
    }

    private static void RequireMaxLength(
        Dictionary<string, List<string>> errors,
        string field,
        string value,
        int maxLength,
        bool required)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (required)
            {
                Add(errors, field, $"{field} is required.");
            }

            return;
        }

        if (value.Trim().Length > maxLength)
        {
            Add(errors, field, $"{field} must be at most {maxLength} characters.");
        }
    }

    private static void Add(Dictionary<string, List<string>> errors, string field, string message)
    {
        if (!errors.TryGetValue(field, out var list))
        {
            list = [];
            errors[field] = list;
        }

        list.Add(message);
    }

    private static void ThrowIfErrors(Dictionary<string, List<string>> errors)
    {
        if (errors.Count == 0)
        {
            return;
        }

        throw new ValidationException(errors.ToDictionary(x => x.Key, x => x.Value.ToArray()));
    }
}
