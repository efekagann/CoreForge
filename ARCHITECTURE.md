# CoreForge Architecture

## Layer Overview

```
┌──────────────────────────────────────────────────────────┐
│                      WebAPI                              │
│         Controllers · Middleware · Program.cs            │
├──────────────────────┬───────────────────────────────────┤
│      Identity        │         Infrastructure            │
│  ASP.NET Identity    │  EF Core · Payments · Email       │
│  JWT · TenantProvider│  Storage · Background Jobs        │
├──────────────────────┴───────────────────────────────────┤
│                     Application                          │
│      CQRS (MediatR) · Validators · AutoMapper            │
│      Interfaces (IPaymentService, IEmailService …)       │
├──────────────────────────────────────────────────────────┤
│                       Domain                             │
│        Entities · Enums · Domain Events · Result<T>      │
└──────────────────────────────────────────────────────────┘
```

Dependency direction: **WebAPI / Identity / Infrastructure → Application → Domain**. Infrastructure and Identity never import each other.

---

## Key Patterns

### CQRS with MediatR

Every feature has dedicated command/query classes:

```
Application/Features/Tenants/
  Commands/CreateTenant/
    CreateTenantCommand.cs          ← IRequest<TenantDto>
    CreateTenantCommandHandler.cs
    CreateTenantCommandValidator.cs ← FluentValidation
  Queries/GetTenants/
    GetTenantsQuery.cs
    GetTenantsQueryHandler.cs
```

Validators run automatically via the `ValidationBehavior<TRequest, TResponse>` pipeline behavior registered in `Application/DependencyInjection.cs`.

### Repository + Unit of Work

```csharp
IRepository<T>   // AddAsync, GetByIdAsync, FindAsync, Remove, Update
IUnitOfWork      // SaveChangesAsync
```

Both are implemented by `GenericRepository<T>` (which also extends `IUnitOfWork` via `ApplicationDbContext`). Registered as open-generics:

```csharp
services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
```

### Global Query Filters

`ApplicationDbContext.ApplyGlobalFilters()` uses reflection to detect which interfaces each entity type implements, then calls the matching private generic method:

| Interfaces | Filter applied |
|------------|---------------|
| `ITenantScopedEntity` only | `e.TenantId == currentTenantId` |
| `ISoftDeletable` only | `e.DeletedAt == null` |
| Both | AND of both conditions |

SuperAdmin sees all tenants because `TenantProvider.CurrentTenantId == null` when no `X-Tenant-Id` header is present.

### Soft Delete

`GenericRepository.Remove()` checks `ISoftDeletable` and sets `DeletedAt` instead of calling `DbSet.Remove()`. `ApplicationDbContext.SaveChangesAsync()` also intercepts any remaining `EntityState.Deleted` entries as a belt-and-suspenders measure.

### Audit Log

`BuildAuditEntries()` runs before `base.SaveChangesAsync()`, capturing `OriginalValues` (before) and `CurrentValues` (after) as JSON for every `Added`, `Modified`, or `Deleted` `BaseEntity` change. `AuditEntry` entities themselves are excluded to prevent infinite loops.

### Multi-Tenancy

`TenantMiddleware` reads the `X-Tenant-Id` request header and calls `ITenantProvider.SetTenant(guid)`. The scoped `TenantProvider` holds `CurrentTenantId` for the lifetime of the request. EF global filters read this value at query time.

### Exception Handling

`ExceptionHandlingMiddleware` catches all `AppException` subclasses, localizes the message via `IStringLocalizer<SharedResource>`, and returns RFC 7807 `ProblemDetails`:

| Exception | HTTP Status |
|-----------|------------|
| `NotFoundException` | 404 |
| `UnauthorizedException` | 401 |
| `ForbiddenException` | 403 |
| `ConflictException` | 409 |
| Unhandled | 500 |

### Payment Provider

`DependencyInjection.cs` reads `"PaymentProvider"` from `appsettings.json` and registers either `MockPaymentService` or `StripePaymentService` as `IPaymentService`. No code change needed to swap providers.

The same pattern applies to:
- `IEmailService` — `Mock` or `MailKit`
- `IStorageService` — `Mock` or `Local`

### Feature Gating

`IFeatureService.IsFeatureEnabled(TenantPlan plan, string featureName)` and `GetLimit(plan, limitName)` give you a static plan-feature matrix. Feature and limit name constants are in `Domain/Common/Features.cs`.

---

## Adding a New Entity

1. Create entity in `CoreForge.Domain/Entities/`:
   ```csharp
   public class Invoice : AuditableEntity, ITenantScopedEntity, ISoftDeletable
   {
       public Guid TenantId { get; set; }
       public DateTime? DeletedAt { get; set; }
       // ...
   }
   ```
   - `ITenantScopedEntity` → global tenant filter applied automatically
   - `ISoftDeletable` → soft-delete applied automatically
   - `AuditableEntity` → CreatedAt / UpdatedAt filled automatically + audit log captured

2. Add EF configuration in `CoreForge.Infrastructure/Persistence/Configurations/InvoiceConfiguration.cs`:
   ```csharp
   public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
   {
       public void Configure(EntityTypeBuilder<Invoice> builder)
       {
           builder.ToTable("Invoices");
           // indexes, constraints...
       }
   }
   ```

3. Add `DbSet` to `IApplicationDbContext` and `ApplicationDbContext`.

4. Create and apply migration:
   ```bash
   dotnet ef migrations add AddInvoices --project ../CoreForge.Infrastructure
   dotnet ef database update --project ../CoreForge.Infrastructure
   ```

5. Add CQRS in `CoreForge.Application/Features/Invoices/`.

6. Add controller in `CoreForge.WebAPI/Controllers/`.

---

## Adding a New Provider

Example: adding an S3 storage provider.

1. Implement `IStorageService` in `CoreForge.Infrastructure/Storage/S3StorageService.cs`.

2. Add config binding in `DependencyInjection.cs`:
   ```csharp
   case "S3":
       services.AddScoped<IStorageService, S3StorageService>();
       break;
   ```

3. Add settings in `appsettings.json` under `"Storage"`.

---

## Request Pipeline

```
HTTPS Request
  → ExceptionHandlingMiddleware    (catches AppException → ProblemDetails)
  → RequestLocalization            (reads Accept-Language header)
  → SerilogRequestLogging
  → HTTPS Redirection
  → RateLimiter                    (Default / Tenant / Auth policies)
  → Authentication                 (JWT Bearer)
  → TenantMiddleware               (X-Tenant-Id → ITenantProvider)
  → Authorization
  → Controller → MediatR → Handler
```

---

## Localization

Resource files: `CoreForge.Application/Common/Localization/`
- `SharedResource.resx` — English (default)
- `SharedResource.tr.resx` — Turkish

Add new messages via `ResourceKeys` constants to keep string keys type-safe. Inject `IStringLocalizer<SharedResource>` anywhere.

---

## Testing

```
tests/
├── CoreForge.Domain.Tests/          ← Entity logic, domain events
├── CoreForge.Application.Tests/     ← Command/query handlers (mock repos)
└── CoreForge.Infrastructure.Tests/  ← EF Core (SQLite in-memory), payment services
```

`IRepository<T>` and `IUnitOfWork` are interfaces — mock them with Moq or NSubstitute in Application tests.
