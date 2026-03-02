using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Infrastructure.Persistence;

namespace SaaS.Infrastructure.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        IUnitOfWork unitOfWork,
        ApplicationDbContext context,
        ICacheService cacheService,
        ILogger<SubscriptionService> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TenantSubscription?> GetActiveSubscriptionAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"tenant_subscription_{tenantId}";
        
        var cached = await _cacheService.GetAsync<TenantSubscription>(cacheKey, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var subscription = await _context.TenantSubscriptions
            .Include(s => s.Plan)
            .Where(s => s.TenantId == tenantId && s.IsActive && s.EndDate > DateTime.UtcNow)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription != null)
        {
            await _cacheService.SetAsync(cacheKey, subscription, TimeSpan.FromMinutes(10), cancellationToken);
        }

        return subscription;
    }

    public async Task<bool> HasFeatureAccessAsync(
        Guid tenantId,
        string featureName,
        CancellationToken cancellationToken = default)
    {
        var subscription = await GetActiveSubscriptionAsync(tenantId, cancellationToken);
        
        if (subscription == null)
        {
            return false;
        }

        return featureName.ToLower() switch
        {
            "api" => subscription.Plan.HasApiAccess,
            "priority_support" => subscription.Plan.HasPrioritySupport,
            "custom_branding" => subscription.Plan.HasCustomBranding,
            _ => false
        };
    }

    public async Task<bool> CanAddUserAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var subscription = await GetActiveSubscriptionAsync(tenantId, cancellationToken);
        
        if (subscription == null)
        {
            return false;
        }

        var currentUserCount = await _context.Users
            .CountAsync(u => u.TenantId == tenantId && u.IsActive, cancellationToken);

        return currentUserCount < subscription.Plan.MaxUsers;
    }

    public async Task<bool> IsSubscriptionActiveAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var subscription = await GetActiveSubscriptionAsync(tenantId, cancellationToken);
        return subscription != null;
    }

    public async Task DeactivateExpiredSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        var expiredSubscriptions = await _context.TenantSubscriptions
            .Where(s => s.IsActive && s.EndDate <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var subscription in expiredSubscriptions)
        {
            subscription.IsActive = false;
            _logger.LogInformation(
                "Deactivated expired subscription {SubscriptionId} for tenant {TenantId}",
                subscription.Id,
                subscription.TenantId);

            // Clear cache
            await _cacheService.RemoveAsync($"tenant_subscription_{subscription.TenantId}", cancellationToken);
        }

        if (expiredSubscriptions.Any())
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
