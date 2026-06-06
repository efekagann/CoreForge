using AutoMapper;
using CoreForge.Application.AuditLog.DTOs;
using CoreForge.Domain.Entities;

namespace CoreForge.Application.AuditLog.Mappings;

public class AuditLogProfile : Profile
{
    public AuditLogProfile()
    {
        CreateMap<AuditEntry, AuditEntryDto>();
    }
}
