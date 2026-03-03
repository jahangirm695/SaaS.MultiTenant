using MediatR;
using SaaS.Application.DTOs;
using SaaS.Application.DTOs.Subscriptions;

namespace SaaS.Application.Queries.Subscriptions;

public record GetSubscriptionPlansQuery : IRequest<ApiResponse<List<SubscriptionPlanDto>>>;

public record GetCurrentSubscriptionQuery : IRequest<ApiResponse<TenantSubscriptionDto>>;