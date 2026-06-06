using AutoMapper;
using CoreForge.Application.Tenants.Mappings;
using CoreForge.Application.Tenants.Queries.GetTenants;
using CoreForge.Application.Tests.Helpers;
using CoreForge.Domain.Entities;
using CoreForge.Domain.Interfaces;

namespace CoreForge.Application.Tests.Tenants;

public class GetTenantsQueryHandlerTests
{
    private readonly IRepository<Tenant> _repo = Substitute.For<IRepository<Tenant>>();
    private readonly IMapper _mapper = MapperFactory.Create(typeof(TenantProfile).Assembly);

    [Fact]
    public async Task Handle_ReturnsAllTenants()
    {
        _repo.GetAllAsync(Arg.Any<CancellationToken>())
             .Returns(new List<Tenant>
             {
                 new() { Name = "Acme",   Slug = "acme",   Plan = TenantPlan.Starter },
                 new() { Name = "Globex", Slug = "globex", Plan = TenantPlan.Professional }
             }.AsReadOnly() as IReadOnlyList<Tenant>);

        var result = await new GetTenantsQueryHandler(_repo, _mapper)
            .Handle(new GetTenantsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, dto => dto.Slug == "acme");
        Assert.Contains(result, dto => dto.Slug == "globex");
    }

    [Fact]
    public async Task Handle_NoTenants_ReturnsEmptyList()
    {
        _repo.GetAllAsync(Arg.Any<CancellationToken>())
             .Returns(new List<Tenant>().AsReadOnly() as IReadOnlyList<Tenant>);

        var result = await new GetTenantsQueryHandler(_repo, _mapper)
            .Handle(new GetTenantsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
