using SaaS.Domain.Common;
using SaaS.Domain.Enums;

namespace SaaS.Domain.Entities;

public class User : BaseAuditableEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    
    public string FullName => $"{FirstName} {LastName}";
}
