using Ecommerce.BasketService.Api.Exceptions;
using Ecommerce.BasketService.Application;
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
            ApiValidationException validationException => Results.ValidationProblem(validationException.Errors),
            BasketValidationException validationException => Results.ValidationProblem(validationException.Errors),
            BasketDomainException domainException when domainException.ErrorCode == "basket_inactive" => Results.Problem(
                title: "Basket conflict",
                detail: domainException.Message,
                statusCode: StatusCodes.Status409Conflict),
            BasketDomainException domainException => Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [domainException.Field] = [domainException.Message]
            }),
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

