using Microsoft.AspNetCore.Http;
using SaaS.Application.Interfaces;

namespace SaaS.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetCurrentTenantId()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var tenantIdClaim = user.FindFirst("TenantId")?.Value;

        if (string.IsNullOrEmpty(tenantIdClaim))
        {
            return null;
        }

        return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : null;
    }

    public async Task<bool> HasAccessToTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var currentTenantId = GetCurrentTenantId();

        if (currentTenantId == null)
        {
            return false;
        }

        return currentTenantId.Value == tenantId;
    }

    public async Task<bool> IsSubdomainAvailableAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        // This method should not be in TenantService as it creates circular dependency
        // Subdomain validation is now handled directly in RegisterCommandHandler using IUnitOfWork
        throw new NotSupportedException("Use IUnitOfWork.Tenants repository to check subdomain availability");
    }
}