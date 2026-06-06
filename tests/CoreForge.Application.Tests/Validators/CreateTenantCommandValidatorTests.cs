using CoreForge.Application.Common.Localization;
using CoreForge.Application.Tenants.Commands.CreateTenant;
using Microsoft.Extensions.Localization;

namespace CoreForge.Application.Tests.Validators;

public class CreateTenantCommandValidatorTests
{
    private static CreateTenantCommandValidator BuildValidator()
    {
        var localizer = Substitute.For<IStringLocalizer<SharedResource>>();
        localizer[Arg.Any<string>()].Returns(x => new LocalizedString((string)x[0], (string)x[0]));
        return new CreateTenantCommandValidator(localizer);
    }

    [Theory]
    [InlineData("acme-corp")]
    [InlineData("my-saas-app")]
    [InlineData("abc")]
    public async Task Slug_ValidFormat_PassesValidation(string slug)
    {
        var validator = BuildValidator();
        var result = await validator.ValidateAsync(new CreateTenantCommand("Name", slug, Domain.Entities.TenantPlan.Free, null));
        Assert.True(result.IsValid, $"Expected valid but got: {string.Join(", ", result.Errors.Select(e => e.ErrorMessage))}");
    }

    [Theory]
    [InlineData("UPPER")]          // uppercase not allowed by regex
    [InlineData("-starts-with-dash")]
    [InlineData("ends-with-dash-")]
    [InlineData("a")]              // too short (min 2 chars enforced by regex)
    public async Task Slug_InvalidFormat_FailsValidation(string slug)
    {
        var validator = BuildValidator();
        var result = await validator.ValidateAsync(new CreateTenantCommand("Name", slug, Domain.Entities.TenantPlan.Free, null));
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Name_Empty_FailsValidation()
    {
        var validator = BuildValidator();
        var result = await validator.ValidateAsync(new CreateTenantCommand("", "valid-slug", Domain.Entities.TenantPlan.Free, null));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task ContactEmail_ValidEmail_PassesValidation()
    {
        var validator = BuildValidator();
        var result = await validator.ValidateAsync(new CreateTenantCommand("Name", "valid-slug", Domain.Entities.TenantPlan.Free, "hello@example.com"));
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ContactEmail_InvalidEmail_FailsValidation()
    {
        var validator = BuildValidator();
        var result = await validator.ValidateAsync(new CreateTenantCommand("Name", "valid-slug", Domain.Entities.TenantPlan.Free, "not-an-email"));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ContactEmail");
    }

    [Fact]
    public async Task ContactEmail_Null_PassesValidation()
    {
        var validator = BuildValidator();
        var result = await validator.ValidateAsync(new CreateTenantCommand("Name", "valid-slug", Domain.Entities.TenantPlan.Free, null));
        Assert.True(result.IsValid);
    }
}
