# CoreForge

**Production-ready SaaS Starter Kit for .NET 9**

Multi-tenancy, JWT auth, Stripe payments, audit log, soft delete, email, background jobs, and rate limiting — all wired up with Clean Architecture and CQRS. Ship your SaaS faster.

---

## Quick Start

**Prerequisites:** .NET 9 SDK, Docker Desktop

```bash
# Clone & setup (Windows)
.\setup.ps1 --seed

# Clone & setup (Linux / macOS)
chmod +x setup.sh && ./setup.sh --seed

# Run
cd src/CoreForge.WebAPI
dotnet run
```

Open Swagger UI: `https://localhost:7001/swagger`

---

## Seeded Accounts

| Role       | Email                  | Password    | Notes                       |
|------------|------------------------|-------------|-----------------------------|
| SuperAdmin | admin@coreforge.com    | Admin@1234! | Full system access           |
| Admin      | admin@acme.com         | Test@1234!  | Acme Corp tenant             |
| User       | user@acme.com          | Test@1234!  | Acme Corp tenant             |
| Admin      | admin@globex.com       | Test@1234!  | Globex Inc tenant            |
| User       | user@globex.com        | Test@1234!  | Globex Inc tenant            |

For tenant-scoped endpoints, send the `X-Tenant-Id: <guid>` header (logged on startup after `--seed`).

---

## Features

| Feature | Details |
|---------|---------|
| **Multi-Tenancy** | Row-based, `X-Tenant-Id` header, global EF query filter, SuperAdmin bypass |
| **Authentication** | JWT (15 min) + Redis refresh tokens (7 days), ASP.NET Identity |
| **Payments** | Mock (default) or Stripe — swap via `appsettings.json` |
| **Subscriptions** | Plan-based (Free / Starter / Professional / Enterprise), feature gating |
| **Audit Log** | Automatic JSON before/after capture for all entity changes |
| **Soft Delete** | `ISoftDeletable` marker → auto-filtered, never actually deleted |
| **Email** | Mock (logs) or MailKit SMTP, HTML template engine (`{{variable}}`) |
| **Background Jobs** | `Channel<T>`-based queue, `IBackgroundJobQueue.QueueAsync()` |
| **Storage** | Local disk or Mock (in-memory) — swap via config |
| **Rate Limiting** | 3 policies: Default (60/min), Tenant (300/min), Auth (10/min brute-force) |
| **Localization** | English + Turkish, `Accept-Language` header, typed `ResourceKeys` |
| **Observability** | Serilog (console + rolling file), `/health` endpoint |

---

## Project Structure

```
CoreForge/
├── src/
│   ├── CoreForge.Domain/          # Entities, interfaces, value objects
│   ├── CoreForge.Application/     # CQRS commands/queries, DTOs, validators
│   ├── CoreForge.Infrastructure/  # EF Core, payments, email, storage, jobs
│   ├── CoreForge.Identity/        # ASP.NET Identity, JWT, tenant provider
│   └── CoreForge.WebAPI/          # Controllers, middleware, Program.cs
├── tests/
│   ├── CoreForge.Domain.Tests/
│   ├── CoreForge.Application.Tests/
│   └── CoreForge.Infrastructure.Tests/
├── postman/
│   └── CoreForge.postman_collection.json
├── docker-compose.yml             # PostgreSQL 17 (port 5433) + Redis 7
├── setup.ps1                      # Windows setup script
└── setup.sh                       # Linux/macOS setup script
```

See [ARCHITECTURE.md](ARCHITECTURE.md) for layer details and extension guide.

---

## Configuration

Edit `src/CoreForge.WebAPI/appsettings.json`:

```json
{
  "DatabaseProvider": "PostgreSQL",
  "PaymentProvider": "Mock",
  "Email": { "Provider": "Mock" },
  "Storage": { "Provider": "Local" }
}
```

Switch to real providers:

| Key | Options |
|-----|---------|
| `DatabaseProvider` | `PostgreSQL` \| `SqlServer` |
| `PaymentProvider` | `Mock` \| `Stripe` |
| `Email.Provider` | `Mock` \| `MailKit` |
| `Storage.Provider` | `Mock` \| `Local` |

For Stripe, fill in `Stripe:SecretKey`, `Stripe:PublishableKey`, `Stripe:WebhookSecret`.

---

## API Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/api/auth/register` | — | Register |
| POST | `/api/auth/login` | — | Login → JWT |
| POST | `/api/auth/refresh` | — | Refresh token |
| POST | `/api/auth/logout` | Bearer | Revoke refresh token |
| GET | `/api/tenants` | SuperAdmin | List tenants |
| POST | `/api/tenants` | SuperAdmin | Create tenant |
| PUT | `/api/tenants/{id}` | SuperAdmin | Update tenant |
| DELETE | `/api/tenants/{id}` | SuperAdmin | Soft-delete tenant |
| POST | `/api/payments/checkout` | Bearer + X-Tenant-Id | Start checkout |
| POST | `/api/payments/webhook` | — | Stripe webhook |
| GET | `/api/payments/history` | Bearer + X-Tenant-Id | Payment history |
| GET | `/api/payments/subscription` | Bearer + X-Tenant-Id | Current subscription |
| GET | `/api/auditlog` | SuperAdmin | Audit log (filtered) |
| GET | `/health` | — | Health check |

---

## Adding a New Feature

1. **Domain** — Add entity in `CoreForge.Domain/Entities/`, implement `ITenantScopedEntity` and/or `ISoftDeletable` as needed
2. **Application** — Add command/query + validator + AutoMapper profile in `CoreForge.Application/Features/<FeatureName>/`
3. **Infrastructure** — Add EF config in `Persistence/Configurations/`, create migration
4. **WebAPI** — Add controller

Global query filters, audit log, and soft delete activate automatically via the interfaces.

---

## Running Migrations

```bash
# From solution root
dotnet tool restore
cd src/CoreForge.WebAPI
dotnet ef migrations add <MigrationName> --project ../CoreForge.Infrastructure
dotnet ef database update --project ../CoreForge.Infrastructure
```

---

## Docker

```bash
docker compose up -d        # Start PostgreSQL + Redis
docker compose down         # Stop
docker compose down -v      # Stop + remove volumes (wipes DB)
```

PostgreSQL: `localhost:5433` (avoids conflict with any local PG on 5432)  
Redis: `localhost:6379`

---

## License

MIT — see LICENSE file.
