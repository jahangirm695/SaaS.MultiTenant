using SaaS.Domain.Common;
using SaaS.Domain.Enums;

namespace SaaS.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public PlanType PlanType { get; set; }
    public decimal Price { get; set; }
    public int MaxUsers { get; set; }
    public int MaxProjects { get; set; }
    public int MaxStorage { get; set; } // In GB
    public bool HasApiAccess { get; set; }
    public bool HasPrioritySupport { get; set; }
    public bool HasCustomBranding { get; set; }
    public string FeaturesJson { get; set; } = "{}"; // JSON string for flexibility
    public bool IsActive { get; set; }
    
    // Navigation properties
    public ICollection<TenantSubscription> TenantSubscriptions { get; set; } = new List<TenantSubscription>();
}
