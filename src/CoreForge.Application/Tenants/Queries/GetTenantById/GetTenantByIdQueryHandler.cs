using AutoMapper;
using CoreForge.Application.Common.Exceptions;
using CoreForge.Application.Common.Localization;
using CoreForge.Application.Tenants.DTOs;
using CoreForge.Domain.Entities;
using CoreForge.Domain.Interfaces;
using MediatR;

namespace CoreForge.Application.Tenants.Queries.GetTenantById;

public class GetTenantByIdQueryHandler(
    IRepository<Tenant> repository,
    IMapper mapper
) : IRequestHandler<GetTenantByIdQuery, TenantDto>
{
    public async Task<TenantDto> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ResourceKeys.Tenant.NotFound);

        return mapper.Map<TenantDto>(tenant);
    }
}
