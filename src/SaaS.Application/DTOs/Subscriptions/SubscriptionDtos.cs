using SaaS.Domain.Enums;

namespace SaaS.Application.DTOs.Subscriptions;

public class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PlanType PlanType { get; set; }
    public decimal Price { get; set; }
    public int MaxUsers { get; set; }
    public int MaxProjects { get; set; }
    public int MaxStorage { get; set; }
    public bool HasApiAccess { get; set; }
    public bool HasPrioritySupport { get; set; }
    public bool HasCustomBranding { get; set; }
}

public class CreateSubscriptionRequest
{
    public Guid PlanId { get; set; }
    public bool AutoRenew { get; set; }
}

public class TenantSubscriptionDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public SubscriptionPlanDto Plan { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool AutoRenew { get; set; }
    public decimal AmountPaid { get; set; }
    public int DaysRemaining { get; set; }
}
