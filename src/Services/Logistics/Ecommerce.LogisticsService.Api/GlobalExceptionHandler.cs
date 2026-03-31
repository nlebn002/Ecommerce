using Ecommerce.LogisticsService.Domain;
using Microsoft.AspNetCore.Diagnostics;

namespace Ecommerce.LogisticsService.Api;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception while processing logistics API request.");

        var result = exception switch
        {
            LogisticsException logisticsException => HandleLogisticsException(logisticsException),
            _ => Results.Problem(title: "Server error", statusCode: StatusCodes.Status500InternalServerError)
        };

        await result.ExecuteAsync(httpContext);
        return true;
    }

    private static IResult HandleLogisticsException(LogisticsException exception)
    {
        return exception.Type switch
        {
            LogisticsErrorType.Validation => Results.ValidationProblem(exception.Errors ?? new Dictionary<string, string[]>
            {
                [string.Empty] = [exception.Message]
            }),
            LogisticsErrorType.NotFound => Results.Problem(
                title: "Shipment not found",
                detail: exception.Message,
                statusCode: StatusCodes.Status404NotFound),
            LogisticsErrorType.Conflict => Results.Problem(
                title: "Shipment conflict",
                detail: exception.Message,
                statusCode: StatusCodes.Status409Conflict),
            _ => Results.Problem(title: "Server error", statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}
