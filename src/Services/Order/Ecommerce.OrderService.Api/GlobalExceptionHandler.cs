using Ecommerce.OrderService.Domain;
using Microsoft.AspNetCore.Diagnostics;

namespace Ecommerce.OrderService.Api;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception while processing order API request.");

        var result = exception switch
        {
            OrderException orderException => HandleOrderException(orderException),
            _ => Results.Problem(title: "Server error", statusCode: StatusCodes.Status500InternalServerError)
        };

        await result.ExecuteAsync(httpContext);
        return true;
    }

    private static IResult HandleOrderException(OrderException exception)
    {
        return exception.Type switch
        {
            OrderErrorType.Validation => Results.ValidationProblem(exception.Errors ?? new Dictionary<string, string[]>
            {
                [string.Empty] = [exception.Message]
            }),
            OrderErrorType.NotFound => Results.Problem(
                title: "Order not found",
                detail: exception.Message,
                statusCode: StatusCodes.Status404NotFound),
            OrderErrorType.Conflict => Results.Problem(
                title: "Order conflict",
                detail: exception.Message,
                statusCode: StatusCodes.Status409Conflict),
            _ => Results.Problem(title: "Server error", statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}
