using CoreForge.Application.Tenants.DTOs;
using MediatR;

namespace CoreForge.Application.Tenants.Queries.GetTenants;

public record GetTenantsQuery : IRequest<IList<TenantDto>>;
