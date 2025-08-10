using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace TodoSystem.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Check if response has already started or been sent
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Cannot handle exception because the response has already started");
            return;
        }

        try
        {
            // Clear any existing response content
            context.Response.Clear();
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Instance = context.Request.Path
            };

            switch (exception)
            {
                case ValidationException validationException:
                    problemDetails.Title = "Validation error(s)";
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Detail = "One or more validation errors occurred.";
                    problemDetails.Extensions["errors"] = validationException.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        );
                    break;

                case KeyNotFoundException:
                    problemDetails.Title = "Resource not found";
                    problemDetails.Status = (int)HttpStatusCode.NotFound;
                    problemDetails.Detail = exception.Message;
                    break;

                case TaskCanceledException when exception.InnerException is TimeoutException:
                case TimeoutException:
                    problemDetails.Title = "Request timeout";
                    problemDetails.Status = (int)HttpStatusCode.RequestTimeout;
                    problemDetails.Detail = "The request timed out. Please try again.";
                    break;

                case HttpRequestException httpRequestException:
                    problemDetails.Title = "External service error";
                    problemDetails.Status = (int)HttpStatusCode.BadGateway;
                    problemDetails.Detail = _environment.IsDevelopment()
                        ? httpRequestException.Message
                        : "An error occurred while communicating with an external service.";
                    break;

                default:
                    problemDetails.Title = "An unexpected error occurred";
                    problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                    problemDetails.Detail = _environment.IsDevelopment()
                        ? exception.ToString()
                        : "An unexpected error occurred. Please try again later.";
                    break;
            }

            context.Response.StatusCode = problemDetails.Status.Value;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            };

            var json = JsonSerializer.Serialize(problemDetails, options);

            // Use a safer write method that handles disposed streams
            if (context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogWarning("Request was cancelled before response could be written");
                return;
            }

            await context.Response.WriteAsync(json, context.RequestAborted);
        }
        catch (ObjectDisposedException)
        {
            _logger.LogWarning("Could not write error response because the response stream was disposed");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("response has already started"))
        {
            _logger.LogWarning("Could not write error response because the response has already started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while handling an exception");
        }
    }
}

// Extension method for middleware registration
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}