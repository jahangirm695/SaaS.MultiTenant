using Microsoft.EntityFrameworkCore;
using SaaS.Application.Interfaces;
using SaaS.Domain.Common;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantService tenantService) : base(options)
    {
        _tenantService = tenantService;
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<TenantSubscription> TenantSubscriptions { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter for multi-tenancy
        // Note: The filter is evaluated at query time, not model creation time
        modelBuilder.Entity<User>().HasQueryFilter(e =>
            !e.IsDeleted &&
            (_tenantService.GetCurrentTenantId() == null || e.TenantId == _tenantService.GetCurrentTenantId()));

        modelBuilder.Entity<AuditLog>().HasQueryFilter(e =>
            !e.IsDeleted &&
            (_tenantService.GetCurrentTenantId() == null || e.TenantId == _tenantService.GetCurrentTenantId()));

        modelBuilder.Entity<TenantSubscription>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.IsDeleted = false;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}