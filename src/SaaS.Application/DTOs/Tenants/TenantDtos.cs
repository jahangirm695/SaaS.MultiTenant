namespace SaaS.Application.DTOs.Tenants;

public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? ContactEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserCount { get; set; }
    public SubscriptionStatusDto? ActiveSubscription { get; set; }
}

public class CreateTenantRequest
{
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
}

public class UpdateTenantRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
}

public class SubscriptionStatusDto
{
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int DaysRemaining { get; set; }
}
