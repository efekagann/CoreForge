using CoreForge.Application.AuditLog.DTOs;
using MediatR;

namespace CoreForge.Application.AuditLog.Queries.GetAuditLog;

public record GetAuditLogQuery(
    Guid? TenantId = null,
    string? EntityName = null,
    int Page = 1,
    int PageSize = 50
) : IRequest<IList<AuditEntryDto>>;
