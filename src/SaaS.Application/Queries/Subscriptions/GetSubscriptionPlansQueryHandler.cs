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
