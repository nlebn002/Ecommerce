using Ecommerce.BasketService.Domain;
using Microsoft.AspNetCore.Diagnostics;

namespace Ecommerce.BasketService.Api;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception while processing basket API request.");

        var result = exception switch
        {
            BasketException basketException => HandleBasketException(basketException),
            _ => Results.Problem(
                title: "Server error",
                statusCode: StatusCodes.Status500InternalServerError)
        };

        await result.ExecuteAsync(httpContext);
        return true;
    }

    private static IResult HandleBasketException(BasketException exception)
    {
        return exception.Type switch
        {
            BasketErrorType.Validation => Results.ValidationProblem(exception.Errors ?? new Dictionary<string, string[]>
            {
                [string.Empty] = [exception.Message]
            }),
            BasketErrorType.NotFound => Results.Problem(
                title: "Basket not found",
                detail: exception.Message,
                statusCode: StatusCodes.Status404NotFound),
            BasketErrorType.Conflict => Results.Problem(
                title: "Basket conflict",
                detail: exception.Message,
                statusCode: StatusCodes.Status409Conflict),
            _ => Results.Problem(
                title: "Server error",
                statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}

