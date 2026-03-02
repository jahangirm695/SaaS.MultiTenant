namespace SaaS.Application.Interfaces;

public interface IAuditService
{
    Task LogActionAsync(
        string action,
        string entityName,
        string? entityId = null,
        string? oldValues = null,
        string? newValues = null,
        CancellationToken cancellationToken = default);
}
