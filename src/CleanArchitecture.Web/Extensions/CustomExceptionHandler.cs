using Ardalis.GuardClauses;
using CleanArchitecture.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
// using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;
using ProblemDetails = FastEndpoints.ProblemDetails;

namespace CleanArchitecture.Web.Extensions;

public class CustomExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case ValidationException:
                await HandleValidationException(httpContext, exception);
                return true;
            case NotFoundException:
                await HandleNotFoundException(httpContext, exception);
                return true;
            case UnauthorizedAccessException:
                await HandleUnauthorizedAccessException(httpContext);
                return true;
            case ForbiddenAccessException:
                await HandleForbiddenAccessException(httpContext);
                return true;
            default:
                return false;
        }
    }

    private async Task HandleValidationException(HttpContext httpContext, Exception ex)
    {
        var exception = (ValidationException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(new ValidationProblemDetails(exception.Errors)
        {
            Status = StatusCodes.Status400BadRequest, Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        });
    }

    private async Task HandleNotFoundException(HttpContext httpContext, Exception ex)
    {
        var exception = (NotFoundException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound, Detail = exception.Message
        });
    }

    private async Task HandleUnauthorizedAccessException(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails { Status = StatusCodes.Status401Unauthorized, });
    }

    private async Task HandleForbiddenAccessException(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails { Status = StatusCodes.Status403Forbidden, });
    }
}
