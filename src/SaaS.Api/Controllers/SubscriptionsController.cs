using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.DTOs;
using SaaS.Application.DTOs.Subscriptions;
using SaaS.Application.Queries.Subscriptions;

namespace SaaS.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubscriptionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all available subscription plans
    /// </summary>
    [HttpGet("plans")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<SubscriptionPlanDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlans()
    {
        var query = new GetSubscriptionPlansQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get current tenant's active subscription
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(typeof(ApiResponse<TenantSubscriptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentSubscription()
    {
        // Implementation would use query
        return Ok();
    }

    /// <summary>
    /// Create or upgrade subscription
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<TenantSubscriptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionRequest request)
    {
        // Implementation would use command
        return Ok();
    }
}
