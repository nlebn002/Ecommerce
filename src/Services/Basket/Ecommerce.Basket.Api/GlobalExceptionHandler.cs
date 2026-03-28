using Ecommerce.Basket.Api.Exceptions;
using Ecommerce.Basket.Application;
using Microsoft.AspNetCore.Diagnostics;

namespace Ecommerce.Basket.Api;

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
            ApiValidationException validationException => Results.ValidationProblem(validationException.Errors),
            BasketValidationException validationException => Results.ValidationProblem(validationException.Errors),
            BasketNotFoundException notFoundException => Results.Problem(
                title: "Basket not found",
                detail: notFoundException.Message,
                statusCode: StatusCodes.Status404NotFound),
            BasketConflictException conflictException => Results.Problem(
                title: "Basket conflict",
                detail: conflictException.Message,
                statusCode: StatusCodes.Status409Conflict),
            _ => Results.Problem(
                title: "Server error",
                statusCode: StatusCodes.Status500InternalServerError)
        };

        await result.ExecuteAsync(httpContext);
        return true;
    }
}
