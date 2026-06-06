using AutoMapper;
using CoreForge.Application.Common.Exceptions;
using CoreForge.Application.Tenants.Commands.CreateTenant;
using CoreForge.Application.Tenants.Mappings;
using CoreForge.Application.Tests.Helpers;
using CoreForge.Domain.Entities;
using CoreForge.Domain.Interfaces;

namespace CoreForge.Application.Tests.Tenants;

public class CreateTenantCommandHandlerTests
{
    private readonly IRepository<Tenant> _repo = Substitute.For<IRepository<Tenant>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = MapperFactory.Create(typeof(TenantProfile).Assembly);

    private CreateTenantCommandHandler CreateHandler() =>
        new(_repo, _uow, _mapper);

    [Fact]
    public async Task Handle_NewSlug_CreatesTenantAndReturnsDto()
    {
        _repo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Tenant, bool>>>(), Arg.Any<CancellationToken>())
             .Returns(new List<Tenant>().AsReadOnly() as IReadOnlyList<Tenant>);

        var result = await CreateHandler().Handle(
            new CreateTenantCommand("Acme Corp", "acme-corp", TenantPlan.Starter, "hello@acme.com"),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Acme Corp", result.Name);
        Assert.Equal("acme-corp", result.Slug);
        Assert.Equal(TenantPlan.Starter, result.Plan);
        await _repo.Received(1).AddAsync(Arg.Is<Tenant>(t => t.Slug == "acme-corp"), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UpperCaseSlug_NormalizesToLower()
    {
        _repo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Tenant, bool>>>(), Arg.Any<CancellationToken>())
             .Returns(new List<Tenant>().AsReadOnly() as IReadOnlyList<Tenant>);

        var result = await CreateHandler().Handle(
            new CreateTenantCommand("Test Co", "TEST-CO", TenantPlan.Free, null),
            CancellationToken.None);

        Assert.Equal("test-co", result.Slug);
    }

    [Fact]
    public async Task Handle_DuplicateSlug_ThrowsConflictException()
    {
        _repo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Tenant, bool>>>(), Arg.Any<CancellationToken>())
             .Returns(new List<Tenant> { new() { Slug = "acme" } }.AsReadOnly() as IReadOnlyList<Tenant>);

        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateHandler().Handle(
                new CreateTenantCommand("Acme", "Acme", TenantPlan.Free, null),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NewTenant_IsActiveTrue()
    {
        _repo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Tenant, bool>>>(), Arg.Any<CancellationToken>())
             .Returns(new List<Tenant>().AsReadOnly() as IReadOnlyList<Tenant>);

        await CreateHandler().Handle(
            new CreateTenantCommand("New Corp", "new-corp", TenantPlan.Enterprise, null),
            CancellationToken.None);

        await _repo.Received(1).AddAsync(
            Arg.Is<Tenant>(t => t.IsActive),
            Arg.Any<CancellationToken>());
    }
}
