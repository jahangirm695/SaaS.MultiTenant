using SaaS.Domain.Entities;

namespace SaaS.Application.Interfaces;

public interface ISubscriptionService
{
    Task<TenantSubscription?> GetActiveSubscriptionAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> HasFeatureAccessAsync(Guid tenantId, string featureName, CancellationToken cancellationToken = default);
    Task<bool> CanAddUserAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> IsSubscriptionActiveAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task DeactivateExpiredSubscriptionsAsync(CancellationToken cancellationToken = default);
}
