using Microsoft.EntityFrameworkCore.Storage;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Tenants = new Repository<Tenant>(context);
        Users = new Repository<User>(context);
        SubscriptionPlans = new Repository<SubscriptionPlan>(context);
        TenantSubscriptions = new Repository<TenantSubscription>(context);
        AuditLogs = new Repository<AuditLog>(context);
    }

    public IRepository<Tenant> Tenants { get; }
    public IRepository<User> Users { get; }
    public IRepository<SubscriptionPlan> SubscriptionPlans { get; }
    public IRepository<TenantSubscription> TenantSubscriptions { get; }
    public IRepository<AuditLog> AuditLogs { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
