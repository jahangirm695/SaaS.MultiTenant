using Microsoft.AspNetCore.Http;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantService _tenantService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(
        IUnitOfWork unitOfWork,
        ITenantService tenantService,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _tenantService = tenantService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogActionAsync(
        string action,
        string entityName,
        string? entityId = null,
        string? oldValues = null,
        string? newValues = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        if (tenantId == null) return;

        var httpContext = _httpContextAccessor.HttpContext;
        var userId = httpContext?.User?.FindFirst("sub")?.Value;

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            UserId = userId != null ? Guid.Parse(userId) : null,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown",
            UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
