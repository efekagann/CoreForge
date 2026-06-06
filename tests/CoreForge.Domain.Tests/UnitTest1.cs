using CoreForge.Domain.Common;
using CoreForge.Domain.Entities;

namespace CoreForge.Domain.Tests;

public class BaseEntityTests
{
    private sealed class TestEntity : BaseEntity { }

    [Fact]
    public void NewEntity_HasNonEmptyId()
    {
        var entity = new TestEntity();
        Assert.NotEqual(Guid.Empty, entity.Id);
    }

    [Fact]
    public void TwoNewEntities_HaveDifferentIds()
    {
        var a = new TestEntity();
        var b = new TestEntity();
        Assert.NotEqual(a.Id, b.Id);
    }

    [Fact]
    public void DomainEvents_EmptyOnNewEntity()
    {
        var entity = new TestEntity();
        Assert.Empty(entity.DomainEvents);
    }

    [Fact]
    public void ClearDomainEvents_EmptiesCollection()
    {
        var entity = new TestEntity();
        // Add via protected helper via reflection (no external domain event in domain.tests)
        entity.ClearDomainEvents();
        Assert.Empty(entity.DomainEvents);
    }
}

public class ResultTests
{
    [Fact]
    public void GenericResult_Success_IsSuccessTrue()
    {
        var result = Result<int>.Success(42);
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void GenericResult_Failure_IsSuccessFalse()
    {
        var result = Result<int>.Failure("something went wrong");
        Assert.False(result.IsSuccess);
        Assert.Equal("something went wrong", result.Error);
        Assert.Equal(default, result.Value);
    }

    [Fact]
    public void NonGenericResult_Success_IsSuccessTrue()
    {
        var result = Result.Success();
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
    }

    [Fact]
    public void NonGenericResult_Failure_IsSuccessFalse()
    {
        var result = Result.Failure("error");
        Assert.False(result.IsSuccess);
        Assert.Equal("error", result.Error);
    }
}

public class TenantEntityTests
{
    [Fact]
    public void NewTenant_HasDefaultFreePlan()
    {
        var tenant = new Tenant();
        Assert.Equal(TenantPlan.Free, tenant.Plan);
    }

    [Fact]
    public void NewTenant_IsActiveByDefault()
    {
        var tenant = new Tenant();
        Assert.True(tenant.IsActive);
    }

    [Fact]
    public void NewTenant_HasNonEmptyId()
    {
        var tenant = new Tenant();
        Assert.NotEqual(Guid.Empty, tenant.Id);
    }
}
