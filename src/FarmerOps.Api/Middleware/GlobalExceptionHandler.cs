using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ApplicationValidationException = FarmerOps.Application.Common.Exceptions.ValidationException;
using FluentValidationException = FluentValidation.ValidationException;

namespace FarmerOps.Api.Middleware;

/// <summary>Maps domain/application exceptions to RFC 7807 problem responses so controllers stay free of try/catch.</summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            ApplicationValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            FluentValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            AuthenticationFailedException => (StatusCodes.Status401Unauthorized, "Authentication failed"),
            ForbiddenAccessException => (StatusCodes.Status403Forbidden, "Access forbidden"),
            DomainException => (StatusCodes.Status400BadRequest, "Business rule violation"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == StatusCodes.Status500InternalServerError ? "An unexpected error occurred." : exception.Message,
            Instance = httpContext.Request.Path
        };

        switch (exception)
        {
            case ApplicationValidationException appEx:
                problemDetails.Extensions["errors"] = appEx.Errors;
                break;
            case FluentValidationException fluentEx:
                problemDetails.Extensions["errors"] = fluentEx.Errors
                    .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                    .ToDictionary(g => g.Key, g => g.ToArray());
                break;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
