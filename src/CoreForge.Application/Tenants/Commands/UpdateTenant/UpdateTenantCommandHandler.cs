using AutoMapper;
using CoreForge.Application.Common.Exceptions;
using CoreForge.Application.Common.Localization;
using CoreForge.Application.Tenants.DTOs;
using CoreForge.Domain.Entities;
using CoreForge.Domain.Interfaces;
using MediatR;

namespace CoreForge.Application.Tenants.Commands.UpdateTenant;

public class UpdateTenantCommandHandler(
    IRepository<Tenant> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper
) : IRequestHandler<UpdateTenantCommand, TenantDto>
{
    public async Task<TenantDto> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ResourceKeys.Tenant.NotFound);

        tenant.Name = request.Name;
        tenant.Plan = request.Plan;
        tenant.IsActive = request.IsActive;
        tenant.ContactEmail = request.ContactEmail;

        repository.Update(tenant);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<TenantDto>(tenant);
    }
}
