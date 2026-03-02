namespace SaaS.Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity
{
    public Guid TenantId { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
