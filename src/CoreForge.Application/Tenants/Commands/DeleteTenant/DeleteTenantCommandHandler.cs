using CoreForge.Application.Common.Exceptions;
using CoreForge.Application.Common.Localization;
using CoreForge.Domain.Entities;
using CoreForge.Domain.Interfaces;
using MediatR;

namespace CoreForge.Application.Tenants.Commands.DeleteTenant;

public class DeleteTenantCommandHandler(
    IRepository<Tenant> repository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteTenantCommand>
{
    public async Task Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ResourceKeys.Tenant.NotFound);

        repository.Remove(tenant);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
