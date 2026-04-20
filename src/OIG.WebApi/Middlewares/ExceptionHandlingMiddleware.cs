using FluentValidation;
using OIG.Domain.Common;
using System.Diagnostics;

namespace OIG.WebApi.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", traceId);

        context.Response.ContentType = "application/problem+json";

        var (statusCode, title, detail, extensions) = ex switch
        {
            ValidationException vex => (
                StatusCodes.Status400BadRequest,
                "Validation error",
                "One or more validation errors occurred.",
                new Dictionary<string, object?>
                {
                    ["errors"] = vex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
                }),

            DomainException dex => (
                StatusCodes.Status400BadRequest,
                "Domain rule violation",
                dex.Message,
                null),

            InvalidOperationException iox => (
                StatusCodes.Status409Conflict,
                "Conflict",
                iox.Message,
                null),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Server error",
                _environment.IsDevelopment() ? ex.ToString() : "Unexpected error occurred.",
                null)
        };

        context.Response.StatusCode = statusCode;

        var problem = Results.Problem(
            title: title,
            detail: detail,
            statusCode: statusCode,
            extensions: extensions is null
                ? new Dictionary<string, object?> { ["traceId"] = traceId }
                : extensions.Append(new KeyValuePair<string, object?>("traceId", traceId))
                           .ToDictionary(k => k.Key, v => v.Value)
        );

        await problem.ExecuteAsync(context);
    }
}