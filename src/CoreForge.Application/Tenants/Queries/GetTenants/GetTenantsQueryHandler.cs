using AutoMapper;
using CoreForge.Application.Tenants.DTOs;
using CoreForge.Domain.Entities;
using CoreForge.Domain.Interfaces;
using MediatR;

namespace CoreForge.Application.Tenants.Queries.GetTenants;

public class GetTenantsQueryHandler(
    IRepository<Tenant> repository,
    IMapper mapper
) : IRequestHandler<GetTenantsQuery, IList<TenantDto>>
{
    public async Task<IList<TenantDto>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await repository.GetAllAsync(cancellationToken);
        return mapper.Map<IList<TenantDto>>(tenants);
    }
}
