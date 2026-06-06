using CoreForge.Application.Common.Exceptions;
using CoreForge.Application.Tenants.Commands.DeleteTenant;
using CoreForge.Domain.Entities;
using CoreForge.Domain.Interfaces;

namespace CoreForge.Application.Tests.Tenants;

public class DeleteTenantCommandHandlerTests
{
    private readonly IRepository<Tenant> _repo = Substitute.For<IRepository<Tenant>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private DeleteTenantCommandHandler CreateHandler() =>
        new(_repo, _uow);

    [Fact]
    public async Task Handle_ExistingTenant_RemovesAndSaves()
    {
        var id = Guid.NewGuid();
        var tenant = new Tenant { Name = "Acme", Slug = "acme" };

        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(tenant);

        await CreateHandler().Handle(new DeleteTenantCommand(id), CancellationToken.None);

        _repo.Received(1).Remove(tenant);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonExistingTenant_ThrowsNotFoundException()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
             .Returns((Tenant?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new DeleteTenantCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
