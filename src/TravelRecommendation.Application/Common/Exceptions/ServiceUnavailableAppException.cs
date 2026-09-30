namespace TravelRecommendation.Application.Common.Exceptions;

public class ServiceUnavailableAppException : AppException
{
    public ServiceUnavailableAppException(string message)
        : base(message, 503)
    {
    }
}
