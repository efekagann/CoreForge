using AutoMapper;
using CoreForge.Application.AuditLog.DTOs;
using CoreForge.Domain.Entities;
using CoreForge.Domain.Interfaces;
using MediatR;

namespace CoreForge.Application.AuditLog.Queries.GetAuditLog;

public class GetAuditLogQueryHandler(
    IRepository<AuditEntry> repository,
    IMapper mapper
) : IRequestHandler<GetAuditLogQuery, IList<AuditEntryDto>>
{
    public async Task<IList<AuditEntryDto>> Handle(GetAuditLogQuery request, CancellationToken cancellationToken)
    {
        var entries = await repository.FindAsync(e =>
            (request.TenantId == null || e.TenantId == request.TenantId) &&
            (request.EntityName == null || e.EntityName == request.EntityName),
            cancellationToken);

        return mapper.Map<IList<AuditEntryDto>>(
            entries
                .OrderByDescending(e => e.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList());
    }
}
