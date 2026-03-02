using SaaS.Domain.Common;

namespace SaaS.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public User? User { get; set; }
}
