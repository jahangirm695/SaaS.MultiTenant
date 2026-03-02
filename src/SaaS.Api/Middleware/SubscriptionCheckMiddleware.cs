using SaaS.Application.DTOs;
using SaaS.Application.Interfaces;
using System.Net;
using System.Text.Json;

namespace SaaS.Api.Middleware;

public class SubscriptionCheckMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SubscriptionCheckMiddleware> _logger;

    public SubscriptionCheckMiddleware(
        RequestDelegate next,
        ILogger<SubscriptionCheckMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ISubscriptionService subscriptionService, ITenantService tenantService)
    {
        // Skip for auth endpoints
        if (context.Request.Path.StartsWithSegments("/api/auth") || 
            context.Request.Path.StartsWithSegments("/api/subscriptions/plans"))
        {
            await _next(context);
            return;
        }

        // Skip for non-authenticated requests
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        var tenantId = tenantService.GetCurrentTenantId();
        
        if (tenantId == null)
        {
            await _next(context);
            return;
        }

        // Check if subscription is active
        var isActive = await subscriptionService.IsSubscriptionActiveAsync(tenantId.Value);
        
        if (!isActive)
        {
            _logger.LogWarning("Access denied: Tenant {TenantId} has no active subscription", tenantId);
            
            context.Response.StatusCode = (int)HttpStatusCode.PaymentRequired;
            context.Response.ContentType = "application/json";
            
            var response = ApiResponse<object>.FailureResponse(
                "Subscription required",
                new List<string> { "Your subscription has expired. Please renew to continue using the service." });

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
            
            return;
        }

        await _next(context);
    }
}
