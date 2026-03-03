using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.DTOs;
using SaaS.Application.DTOs.Subscriptions;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Application.Commands.Subscriptions;

public class CreateSubscriptionCommandHandler
    : IRequestHandler<CreateSubscriptionCommand, ApiResponse<TenantSubscriptionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantService _tenantService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CreateSubscriptionCommandHandler> _logger;

    public CreateSubscriptionCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantService tenantService,
        ICacheService cacheService,
        ILogger<CreateSubscriptionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantService = tenantService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<ApiResponse<TenantSubscriptionDto>> Handle(
        CreateSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        if (tenantId == null)
        {
            return ApiResponse<TenantSubscriptionDto>.FailureResponse(
                "Tenant not found",
                new List<string> { "Unable to determine current tenant." });
        }

        // Get the plan
        var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(request.PlanId, cancellationToken);
        if (plan == null || !plan.IsActive)
        {
            return ApiResponse<TenantSubscriptionDto>.FailureResponse(
                "Invalid plan",
                new List<string> { "The selected subscription plan is not available." });
        }

        // Deactivate existing active subscriptions
        var existingSubscriptions = await _unitOfWork.TenantSubscriptions.FindAsync(
            s => s.TenantId == tenantId.Value && s.IsActive,
            cancellationToken);

        foreach (var existing in existingSubscriptions)
        {
            existing.IsActive = false;
            await _unitOfWork.TenantSubscriptions.UpdateAsync(existing, cancellationToken);
        }

        // Create new subscription
        var subscription = new TenantSubscription
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            PlanId = request.PlanId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1), // Default 1 month
            IsActive = true,
            AutoRenew = request.AutoRenew,
            AmountPaid = plan.Price,
            CreatedAt = DateTime.UtcNow
        };

        // Add the subscription to the database
        await _unitOfWork.TenantSubscriptions.AddAsync(subscription, cancellationToken);

        // Save all changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created subscription {SubscriptionId} for tenant {TenantId} with plan {PlanId}",
            subscription.Id,
            tenantId.Value,
            request.PlanId);

        // Clear cache
        await _cacheService.RemoveAsync($"tenant_subscription_{tenantId.Value}", cancellationToken);

        var dto = new TenantSubscriptionDto
        {
            Id = subscription.Id,
            TenantId = subscription.TenantId,
            Plan = new SubscriptionPlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                PlanType = plan.PlanType,
                Price = plan.Price,
                MaxUsers = plan.MaxUsers,
                MaxProjects = plan.MaxProjects,
                MaxStorage = plan.MaxStorage,
                HasApiAccess = plan.HasApiAccess,
                HasPrioritySupport = plan.HasPrioritySupport,
                HasCustomBranding = plan.HasCustomBranding
            },
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            IsActive = subscription.IsActive,
            AutoRenew = subscription.AutoRenew,
            AmountPaid = subscription.AmountPaid,
            DaysRemaining = (int)(subscription.EndDate - DateTime.UtcNow).TotalDays
        };

        return ApiResponse<TenantSubscriptionDto>.SuccessResponse(
            dto,
            "Subscription created successfully");
    }
}