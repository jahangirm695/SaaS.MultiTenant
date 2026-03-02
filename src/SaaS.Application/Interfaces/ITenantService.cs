namespace SaaS.Application.Interfaces;

public interface ITenantService
{
    Guid? GetCurrentTenantId();
    Task<bool> HasAccessToTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> IsSubdomainAvailableAsync(string subdomain, CancellationToken cancellationToken = default);
}
