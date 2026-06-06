using CoreForge.Domain.Common;
using CoreForge.Domain.Entities;
using CoreForge.Domain.Interfaces;
using CoreForge.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoreForge.Infrastructure.Seeding;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<DataSeederMarker>>();

        var tenantRepo = services.GetRequiredService<IRepository<Tenant>>();
        var subRepo = services.GetRequiredService<IRepository<Subscription>>();
        var txnRepo = services.GetRequiredService<IRepository<PaymentTransaction>>();
        var unitOfWork = services.GetRequiredService<IUnitOfWork>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Idempotency check
        var existing = await tenantRepo.FindAsync(t => t.Slug == "acme");
        if (existing.Count > 0)
        {
            logger.LogInformation("Seed data already exists. Skipping.");
            return;
        }

        logger.LogInformation("Seeding sample data...");

        // ── SuperAdmin ─────────────────────────────────────────────────────────
        await CreateUserAsync(userManager, logger,
            email: "admin@coreforge.com",
            firstName: "Super", lastName: "Admin",
            password: "Admin@1234!",
            roles: [Roles.SuperAdmin]);

        // ── Tenant 1: Acme Corp ────────────────────────────────────────────────
        var acme = new Tenant
        {
            Name = "Acme Corp",
            Slug = "acme",
            Plan = TenantPlan.Starter,
            ContactEmail = "hello@acme.example",
            IsActive = true
        };
        await tenantRepo.AddAsync(acme);
        await unitOfWork.SaveChangesAsync();

        await CreateUserAsync(userManager, logger,
            email: "admin@acme.com",
            firstName: "Alice", lastName: "Smith",
            password: "Test@1234!",
            roles: [Roles.Admin]);

        await CreateUserAsync(userManager, logger,
            email: "user@acme.com",
            firstName: "Bob", lastName: "Jones",
            password: "Test@1234!",
            roles: [Roles.User]);

        await subRepo.AddAsync(new Subscription
        {
            TenantId = acme.Id,
            Plan = TenantPlan.Starter,
            Status = SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-30),
            ExternalSubscriptionId = "mock_acme_sub"
        });
        await txnRepo.AddAsync(new PaymentTransaction
        {
            TenantId = acme.Id,
            Amount = 29m,
            Currency = "USD",
            Status = PaymentStatus.Succeeded,
            Plan = TenantPlan.Starter,
            Description = "Starter plan — initial",
            ExternalTransactionId = "mock_txn_acme_001"
        });

        // ── Tenant 2: Globex Inc ──────────────────────────────────────────────
        var globex = new Tenant
        {
            Name = "Globex Inc",
            Slug = "globex",
            Plan = TenantPlan.Professional,
            ContactEmail = "hello@globex.example",
            IsActive = true
        };
        await tenantRepo.AddAsync(globex);
        await unitOfWork.SaveChangesAsync();

        await CreateUserAsync(userManager, logger,
            email: "admin@globex.com",
            firstName: "Carol", lastName: "White",
            password: "Test@1234!",
            roles: [Roles.Admin]);

        await CreateUserAsync(userManager, logger,
            email: "user@globex.com",
            firstName: "Dave", lastName: "Brown",
            password: "Test@1234!",
            roles: [Roles.User]);

        await subRepo.AddAsync(new Subscription
        {
            TenantId = globex.Id,
            Plan = TenantPlan.Professional,
            Status = SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-15),
            ExternalSubscriptionId = "mock_globex_sub"
        });
        await txnRepo.AddAsync(new PaymentTransaction
        {
            TenantId = globex.Id,
            Amount = 79m,
            Currency = "USD",
            Status = PaymentStatus.Succeeded,
            Plan = TenantPlan.Professional,
            Description = "Professional plan — initial",
            ExternalTransactionId = "mock_txn_globex_001"
        });

        await unitOfWork.SaveChangesAsync();
        logger.LogInformation("Seed data created successfully.");
        logger.LogInformation("  SuperAdmin : admin@coreforge.com / Admin@1234!");
        logger.LogInformation("  Acme Admin : admin@acme.com      / Test@1234!");
        logger.LogInformation("  Acme User  : user@acme.com       / Test@1234!  (X-Tenant-Id: {AcmeId})", acme.Id);
        logger.LogInformation("  Globex Admin: admin@globex.com   / Test@1234!");
        logger.LogInformation("  Globex User: user@globex.com     / Test@1234!  (X-Tenant-Id: {GlobexId})", globex.Id);
    }

    private static async Task CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        string email, string firstName, string lastName,
        string password, string[] roles)
    {
        if (await userManager.FindByEmailAsync(email) is not null) return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            logger.LogWarning("Could not create user {Email}: {Errors}", email,
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        foreach (var role in roles)
            await userManager.AddToRoleAsync(user, role);
    }

    private sealed class DataSeederMarker;
}
