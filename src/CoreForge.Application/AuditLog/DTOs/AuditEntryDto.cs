using CoreForge.Domain.Common;

namespace CoreForge.Application.AuditLog.DTOs;

public record AuditEntryDto(
    Guid Id,
    string EntityName,
    string EntityId,
    AuditAction Action,
    string? OldValues,
    string? NewValues,
    string? ChangedBy,
    Guid? TenantId,
    DateTime CreatedAt
);
