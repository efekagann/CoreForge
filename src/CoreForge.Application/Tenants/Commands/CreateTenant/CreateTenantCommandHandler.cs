using AutoMapper;
using CoreForge.Application.Common.Exceptions;
using CoreForge.Application.Common.Localization;
using CoreForge.Application.Tenants.DTOs;
using CoreForge.Domain.Entities;
using CoreForge.Domain.Interfaces;
using MediatR;

namespace CoreForge.Application.Tenants.Commands.CreateTenant;

public class CreateTenantCommandHandler(
    IRepository<Tenant> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper
) : IRequestHandler<CreateTenantCommand, TenantDto>
{
    public async Task<TenantDto> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var slug = request.Slug.ToLowerInvariant();
        var existing = await repository.FindAsync(t => t.Slug == slug, cancellationToken);

        if (existing.Count > 0)
            throw new ConflictException(ResourceKeys.Tenant.AlreadyExists);

        var tenant = new Tenant
        {
            Name = request.Name,
            Slug = slug,
            Plan = request.Plan,
            ContactEmail = request.ContactEmail,
            IsActive = true
        };

        await repository.AddAsync(tenant, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<TenantDto>(tenant);
    }
}
