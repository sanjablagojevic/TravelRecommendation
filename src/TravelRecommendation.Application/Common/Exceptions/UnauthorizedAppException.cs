namespace TravelRecommendation.Application.Common.Exceptions;

public class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message = "Unauthorized.")
        : base(message, 401)
    {
    }
}
