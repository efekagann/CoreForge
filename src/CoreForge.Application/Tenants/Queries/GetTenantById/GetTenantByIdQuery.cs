using CoreForge.Application.Tenants.DTOs;
using MediatR;

namespace CoreForge.Application.Tenants.Queries.GetTenantById;

public record GetTenantByIdQuery(Guid Id) : IRequest<TenantDto>;
