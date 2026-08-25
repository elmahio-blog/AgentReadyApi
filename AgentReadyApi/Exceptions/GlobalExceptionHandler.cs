using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AgentReadyApi.Exceptions;
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = exception is ApiException apiException
            ? apiException.StatusCode
            : StatusCodes.Status500InternalServerError;

        var errorCode = exception is ApiException apiEx
            ? apiEx.ErrorCode
            : "INTERNAL_SERVER_ERROR";

        httpContext.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,

            Title = exception is ApiException
                ? exception.Message
                : "An unexpected error occurred.",

            Detail = exception is ApiException
                ? exception.Message
                : "An unexpected error occurred.",

            Type = $"https://api.example.com/problems/{errorCode}"
        };

        problem.Extensions["errorCode"] = errorCode;

        await _problemDetailsService.WriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem
            });

        return true;
    }
    /*
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = exception is ApiException apiException
            ? apiException.StatusCode
            : StatusCodes.Status500InternalServerError;

        var errorCode = exception is ApiException apiEx
            ? apiEx.ErrorCode
            : "INTERNAL_SERVER_ERROR";

        httpContext.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,

            Title = exception is ApiException
                ? exception.Message
                : "An unexpected error occurred.",

            Detail = exception is ApiException
                ? exception.Message
                : "An unexpected error occurred.",

            Type =
                $"https://api.example.com/problems/{errorCode}"
        };

        problem.Extensions["errorCode"] = errorCode;

        return await _problemDetailsService.WriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem
            });
    }
    */
    
}