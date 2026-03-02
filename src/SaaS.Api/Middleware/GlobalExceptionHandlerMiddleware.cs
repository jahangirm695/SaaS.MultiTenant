using FluentValidation;
using SaaS.Application.DTOs;
using System.Net;
using System.Text.Json;

namespace SaaS.Api.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ValidationException validationException => new
            {
                statusCode = (int)HttpStatusCode.BadRequest,
                response = ApiResponse<object>.FailureResponse(
                    "Validation failed",
                    validationException.Errors.Select(e => e.ErrorMessage).ToList())
            },
            UnauthorizedAccessException => new
            {
                statusCode = (int)HttpStatusCode.Unauthorized,
                response = ApiResponse<object>.FailureResponse(
                    "Unauthorized access",
                    new List<string> { "You do not have permission to perform this action" })
            },
            KeyNotFoundException => new
            {
                statusCode = (int)HttpStatusCode.NotFound,
                response = ApiResponse<object>.FailureResponse(
                    "Resource not found",
                    new List<string> { exception.Message })
            },
            _ => new
            {
                statusCode = (int)HttpStatusCode.InternalServerError,
                response = ApiResponse<object>.FailureResponse(
                    "An error occurred while processing your request",
                    new List<string> { exception.Message })
            }
        };

        context.Response.StatusCode = response.statusCode;

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(response.response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    }
}
