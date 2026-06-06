using AutoMapper;
using CoreForge.Application.Common.Exceptions;
using CoreForge.Application.Tenants.Mappings;
using CoreForge.Application.Tenants.Queries.GetTenantById;
using CoreForge.Application.Tests.Helpers;
using CoreForge.Domain.Entities;
using CoreForge.Domain.Interfaces;

namespace CoreForge.Application.Tests.Tenants;

public class GetTenantByIdQueryHandlerTests
{
    private readonly IRepository<Tenant> _repo = Substitute.For<IRepository<Tenant>>();
    private readonly IMapper _mapper = MapperFactory.Create(typeof(TenantProfile).Assembly);

    [Fact]
    public async Task Handle_ExistingId_ReturnsTenantDto()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>())
             .Returns(new Tenant { Name = "Acme", Slug = "acme", Plan = TenantPlan.Starter });

        var result = await new GetTenantByIdQueryHandler(_repo, _mapper)
            .Handle(new GetTenantByIdQuery(id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("acme", result.Slug);
    }

    [Fact]
    public async Task Handle_NonExistingId_ThrowsNotFoundException()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
             .Returns((Tenant?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new GetTenantByIdQueryHandler(_repo, _mapper)
                .Handle(new GetTenantByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }
}
