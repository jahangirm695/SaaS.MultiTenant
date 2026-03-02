namespace SaaS.Api.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Resolve tenant from header or subdomain
        var tenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        
        if (!string.IsNullOrEmpty(tenantId))
        {
            context.Items["TenantId"] = tenantId;
            _logger.LogInformation("Tenant resolved from header: {TenantId}", tenantId);
        }
        else
        {
            // Could also resolve from subdomain
            var host = context.Request.Host.Host;
            var subdomain = ExtractSubdomain(host);
            
            if (!string.IsNullOrEmpty(subdomain))
            {
                context.Items["Subdomain"] = subdomain;
                _logger.LogInformation("Subdomain detected: {Subdomain}", subdomain);
            }
        }

        await _next(context);
    }

    private string? ExtractSubdomain(string host)
    {
        if (string.IsNullOrEmpty(host) || host == "localhost")
        {
            return null;
        }

        var parts = host.Split('.');
        
        // For domain like: tenant.app.com
        if (parts.Length > 2)
        {
            return parts[0];
        }

        return null;
    }
}
