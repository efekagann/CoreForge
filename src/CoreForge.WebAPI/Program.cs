using CoreForge.Application;
using CoreForge.Identity;
using CoreForge.Infrastructure;
using CoreForge.Infrastructure.Persistence;
using CoreForge.Infrastructure.Seeding;
using CoreForge.WebAPI.Middleware;
using CoreForge.WebAPI.RateLimit;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/coreforge-.log", rollingInterval: RollingInterval.Day));

    // Localization — tr ve en desteklenir; varsayılan en
    builder.Services.AddLocalization(options => options.ResourcesPath = "");
    builder.Services.Configure<RequestLocalizationOptions>(options =>
    {
        var supported = new[] { "en", "tr" };
        options.SetDefaultCulture("en")
               .AddSupportedCultures(supported)
               .AddSupportedUICultures(supported);
    });

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddIdentityServices(builder.Configuration);

    // Rate Limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        // Default: IP-based sliding window (60 req/min)
        options.AddSlidingWindowLimiter(RateLimitPolicies.Default, opt =>
        {
            opt.PermitLimit = 60;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.SegmentsPerWindow = 6;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 5;
        });

        // Tenant: X-Tenant-Id header based sliding window (300 req/min)
        options.AddPolicy(RateLimitPolicies.Tenant, httpContext =>
        {
            var tenantId = httpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault()
                           ?? httpContext.Connection.RemoteIpAddress?.ToString()
                           ?? "anonymous";

            return RateLimitPartition.GetSlidingWindowLimiter(tenantId, _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 300,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            });
        });

        // Auth endpoints: stricter (10 req/min per IP) to mitigate brute force
        options.AddPolicy(RateLimitPolicies.Auth, httpContext =>
        {
            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
            return RateLimitPartition.GetFixedWindowLimiter($"auth_{ip}", _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
        });
    });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new()
        {
            Title = "CoreForge API",
            Version = "v1",
            Description = "Production-ready SaaS Starter Kit — Multi-tenancy, JWT Auth, Payments, Audit Log",
            Contact = new() { Name = "CoreForge", Email = "support@coreforge.dev" }
        });

        // JWT Bearer auth button
        c.AddSecurityDefinition("Bearer", new()
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Enter your JWT token. Example: Bearer eyJhbGci..."
        });
        c.AddSecurityRequirement(new()
        {
            {
                new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
                Array.Empty<string>()
            }
        });

        // Include XML comments
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            c.IncludeXmlComments(xmlPath);
    });

    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreForge API v1");
            c.DisplayRequestDuration();
            c.EnableDeepLinking();
        });
        app.UseDeveloperExceptionPage();
    }

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseRequestLocalization();
    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseMiddleware<TenantMiddleware>();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    // Startup: migrate + seed roles (always) + seed data (--seed flag)
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        await SeedRolesAsync(scope.ServiceProvider);

        if (args.Contains("--seed"))
        {
            await DataSeeder.SeedAsync(scope.ServiceProvider);
            Log.Information("Seed complete. You can now start the app without --seed.");
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static async Task SeedRolesAsync(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<CoreForge.Infrastructure.Identity.ApplicationRole>>();
    var roles = new[] { CoreForge.Domain.Common.Roles.SuperAdmin, CoreForge.Domain.Common.Roles.Admin, CoreForge.Domain.Common.Roles.User };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new CoreForge.Infrastructure.Identity.ApplicationRole(role));
    }
}

// Required for WebApplicationFactory in integration tests
public partial class Program { }
