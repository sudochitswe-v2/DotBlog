using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Infrastructure.ExceptionHandler;

public class ExceptionMiddleware(ILogger<ExceptionMiddleware> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is FluentValidation.ValidationException validationException)
        {
            var prop = CreateProblemDetails(validationException);
            httpContext.Response.ContentType = MediaTypeNames.Application.Json;
            if (prop.Status != null) httpContext.Response.StatusCode = prop.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(prop, cancellationToken: cancellationToken);
        }
        else
        {
            var prop = CreateProblemDetails(exception);
            httpContext.Response.ContentType = MediaTypeNames.Application.Json;
            if (prop.Status != null) httpContext.Response.StatusCode = prop.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(prop, cancellationToken: cancellationToken);
        }

        return true;
    }

    private ProblemDetails CreateProblemDetails(Exception exception)
    {
        logger.LogError("internal server error occurred : {Message}", exception.Message);
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = exception.Message,
        };
        return problemDetails;
    }

    private ProblemDetails CreateProblemDetails(FluentValidation.ValidationException exception)
    {
        logger.LogError("validation error occurred : {Message}", exception.Message);
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Error",
            Detail = string.Join(" ", exception.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")),
        };
        return problemDetails;
    }
}