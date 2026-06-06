using AutoMapper;
using CoreForge.Application.Tenants.DTOs;
using CoreForge.Domain.Entities;

namespace CoreForge.Application.Tenants.Mappings;

public class TenantProfile : Profile
{
    public TenantProfile()
    {
        CreateMap<Tenant, TenantDto>();
    }
}
