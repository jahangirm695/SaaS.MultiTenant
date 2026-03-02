using SaaS.Domain.Entities;

namespace SaaS.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Tenant> Tenants { get; }
    IRepository<User> Users { get; }
    IRepository<SubscriptionPlan> SubscriptionPlans { get; }
    IRepository<TenantSubscription> TenantSubscriptions { get; }
    IRepository<AuditLog> AuditLogs { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
