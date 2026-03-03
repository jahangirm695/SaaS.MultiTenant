using MediatR;
using SaaS.Application.DTOs;
using SaaS.Application.DTOs.Subscriptions;

namespace SaaS.Application.Commands.Subscriptions;

public record CreateSubscriptionCommand(Guid PlanId, bool AutoRenew)
    : IRequest<ApiResponse<TenantSubscriptionDto>>;