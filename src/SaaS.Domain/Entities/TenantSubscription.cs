using SaaS.Domain.Common;

namespace SaaS.Domain.Entities;

public class TenantSubscription : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid PlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool AutoRenew { get; set; }
    public decimal AmountPaid { get; set; }
    public string? PaymentTransactionId { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public SubscriptionPlan Plan { get; set; } = null!;
    
    public bool IsExpired => DateTime.UtcNow > EndDate;
    public int DaysRemaining => (EndDate - DateTime.UtcNow).Days;
}
