using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Subdomain)
            .IsRequired()
            .HasMaxLength(63);

        builder.HasIndex(t => t.Subdomain)
            .IsUnique();

        builder.HasIndex(t => t.IsActive);

        builder.HasMany(t => t.Users)
            .WithOne(u => u.Tenant)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Subscriptions)
            .WithOne(s => s.Tenant)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(u => new { u.Email, u.TenantId })
            .IsUnique();

        builder.HasIndex(u => u.TenantId);
        builder.HasIndex(u => u.IsActive);

        builder.Ignore(u => u.FullName);
    }
}

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Price)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(p => p.PlanType);
        builder.HasIndex(p => p.IsActive);
    }
}

public class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.AmountPaid)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(s => new { s.TenantId, s.IsActive });
        builder.HasIndex(s => s.EndDate);

        builder.HasOne(s => s.Tenant)
            .WithMany(t => t.Subscriptions)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Plan)
            .WithMany(p => p.TenantSubscriptions)
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(s => s.IsExpired);
        builder.Ignore(s => s.DaysRemaining);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(a => new { a.TenantId, a.CreatedAt });
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.Action);
    }
}
