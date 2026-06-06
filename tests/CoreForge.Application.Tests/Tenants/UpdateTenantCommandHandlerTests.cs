using AutoMapper;
using CoreForge.Application.Common.Exceptions;
using CoreForge.Application.Tenants.Commands.UpdateTenant;
using CoreForge.Application.Tenants.Mappings;
using CoreForge.Application.Tests.Helpers;
using CoreForge.Domain.Entities;
using CoreForge.Domain.Interfaces;

namespace CoreForge.Application.Tests.Tenants;

public class UpdateTenantCommandHandlerTests
{
    private readonly IRepository<Tenant> _repo = Substitute.For<IRepository<Tenant>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = MapperFactory.Create(typeof(TenantProfile).Assembly);

    [Fact]
    public async Task Handle_ExistingTenant_UpdatesFields()
    {
        var id = Guid.NewGuid();
        var tenant = new Tenant { Name = "Old Name", Plan = TenantPlan.Free, IsActive = true };
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(tenant);

        await new UpdateTenantCommandHandler(_repo, _uow, _mapper)
            .Handle(new UpdateTenantCommand(id, "New Name", TenantPlan.Professional, false, "new@example.com"),
                    CancellationToken.None);

        Assert.Equal("New Name", tenant.Name);
        Assert.Equal(TenantPlan.Professional, tenant.Plan);
        Assert.False(tenant.IsActive);
        Assert.Equal("new@example.com", tenant.ContactEmail);
        _repo.Received(1).Update(tenant);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonExistingTenant_ThrowsNotFoundException()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
             .Returns((Tenant?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new UpdateTenantCommandHandler(_repo, _uow, _mapper)
                .Handle(new UpdateTenantCommand(Guid.NewGuid(), "X", TenantPlan.Free, true, null),
                        CancellationToken.None));
    }
}
