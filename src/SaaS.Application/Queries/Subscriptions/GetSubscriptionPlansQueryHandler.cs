using MediatR;
using SaaS.Application.DTOs;
using SaaS.Application.DTOs.Subscriptions;
using SaaS.Application.Interfaces;

namespace SaaS.Application.Queries.Subscriptions;

public class GetSubscriptionPlansQueryHandler
    : IRequestHandler<GetSubscriptionPlansQuery, ApiResponse<List<SubscriptionPlanDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public GetSubscriptionPlansQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<ApiResponse<List<SubscriptionPlanDto>>> Handle(
        GetSubscriptionPlansQuery request,
        CancellationToken cancellationToken)
    {
        const string cacheKey = "subscription_plans_all";

        // Try cache first
        var cachedPlans = await _cacheService.GetAsync<List<SubscriptionPlanDto>>(cacheKey, cancellationToken);
        if (cachedPlans != null)
        {
            return ApiResponse<List<SubscriptionPlanDto>>.SuccessResponse(cachedPlans);
        }

        var plans = await _unitOfWork.SubscriptionPlans.FindAsync(p => p.IsActive, cancellationToken);

        var planDtos = plans.Select(p => new SubscriptionPlanDto
        {
            Id = p.Id,
            Name = p.Name,
            PlanType = p.PlanType,
            Price = p.Price,
            MaxUsers = p.MaxUsers,
            MaxProjects = p.MaxProjects,
            MaxStorage = p.MaxStorage,
            HasApiAccess = p.HasApiAccess,
            HasPrioritySupport = p.HasPrioritySupport,
            HasCustomBranding = p.HasCustomBranding
        }).OrderBy(p => p.Price).ToList();

        // Cache for 1 hour
        await _cacheService.SetAsync(cacheKey, planDtos, TimeSpan.FromHours(1), cancellationToken);

        return ApiResponse<List<SubscriptionPlanDto>>.SuccessResponse(planDtos);
    }
}

public class GetCurrentSubscriptionQueryHandler
    : IRequestHandler<GetCurrentSubscriptionQuery, ApiResponse<TenantSubscriptionDto>>
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ITenantService _tenantService;

    public GetCurrentSubscriptionQueryHandler(
        ISubscriptionService subscriptionService,
        ITenantService tenantService)
    {
        _subscriptionService = subscriptionService;
        _tenantService = tenantService;
    }

    public async Task<ApiResponse<TenantSubscriptionDto>> Handle(
        GetCurrentSubscriptionQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        if (tenantId == null)
        {
            return ApiResponse<TenantSubscriptionDto>.FailureResponse(
                "Tenant not found",
                new List<string> { "Unable to determine current tenant." });
        }

        var subscription = await _subscriptionService.GetActiveSubscriptionAsync(
            tenantId.Value,
            cancellationToken);

        if (subscription == null)
        {
            return ApiResponse<TenantSubscriptionDto>.FailureResponse(
                "No active subscription found",
                new List<string> { "This tenant does not have an active subscription." });
        }

        var dto = new TenantSubscriptionDto
        {
            Id = subscription.Id,
            TenantId = subscription.TenantId,
            Plan = new SubscriptionPlanDto
            {
                Id = subscription.Plan.Id,
                Name = subscription.Plan.Name,
                PlanType = subscription.Plan.PlanType,
                Price = subscription.Plan.Price,
                MaxUsers = subscription.Plan.MaxUsers,
                MaxProjects = subscription.Plan.MaxProjects,
                MaxStorage = subscription.Plan.MaxStorage,
                HasApiAccess = subscription.Plan.HasApiAccess,
                HasPrioritySupport = subscription.Plan.HasPrioritySupport,
                HasCustomBranding = subscription.Plan.HasCustomBranding
            },
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            IsActive = subscription.IsActive,
            AutoRenew = subscription.AutoRenew,
            AmountPaid = subscription.AmountPaid,
            DaysRemaining = (int)(subscription.EndDate - DateTime.UtcNow).TotalDays
        };

        return ApiResponse<TenantSubscriptionDto>.SuccessResponse(dto);
    }
}