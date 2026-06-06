using Microsoft.AspNetCore.Mvc.Testing;

namespace CoreForge.Integration.Tests;

/// <summary>
/// Integration tests require running Docker containers:
///   docker compose up -d
///
/// Run with:
///   dotnet test tests/CoreForge.Integration.Tests -- --filter Category=Integration
/// </summary>
[Trait("Category", "Integration")]
public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Testing");
        });
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOkOrServiceUnavailable()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");

        // 200 (Healthy) or 503 (Unhealthy — DB unavailable in CI without Docker) are both acceptable
        Assert.True(
            response.StatusCode == System.Net.HttpStatusCode.OK
            || response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable,
            $"Unexpected status: {response.StatusCode}");
    }
}
