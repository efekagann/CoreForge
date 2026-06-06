using CoreForge.Application.Common.Interfaces;

namespace CoreForge.WebAPI.Middleware;

public class TenantMiddleware(RequestDelegate next)
{
    private const string TenantHeader = "X-Tenant-Id";

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        if (!context.Request.Headers.TryGetValue(TenantHeader, out var tenantValue)
            || string.IsNullOrWhiteSpace(tenantValue))
        {
            await next(context);
            return;
        }

        if (!Guid.TryParse(tenantValue, out var tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(
                $"{{\"status\":400,\"title\":\"Bad Request\",\"detail\":\"'{tenantValue}' is not a valid tenant ID.\"}}");
            return;
        }

        tenantProvider.SetTenant(tenantId);
        await next(context);
    }
}
