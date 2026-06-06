using CoreForge.Application.Common.Interfaces;
using CoreForge.Domain.Common;
using CoreForge.Domain.Entities;
using CoreForge.Domain.Interfaces;
using CoreForge.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json;

namespace CoreForge.Infrastructure.Persistence;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext, IUnitOfWork
{
    private readonly ITenantProvider? _tenantProvider;
    private readonly ICurrentUserService? _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantProvider? tenantProvider = null,
        ICurrentUserService? currentUserService = null)
        : base(options)
    {
        _tenantProvider = tenantProvider;
        _currentUserService = currentUserService;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Ignore<DomainEvent>();
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        ApplyGlobalFilters(builder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1 — Audit fields
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        // 2 — Soft delete: intercept Remove() and set DeletedAt instead
        foreach (var entry in ChangeTracker.Entries()
            .Where(e => e.Entity is ISoftDeletable && e.State == EntityState.Deleted))
        {
            entry.State = EntityState.Modified;
            ((ISoftDeletable)entry.Entity).DeletedAt = DateTime.UtcNow;
        }

        // 3 — Build audit entries before saving (captures OriginalValues)
        var auditEntries = BuildAuditEntries();

        if (auditEntries.Count > 0)
            await Set<AuditEntry>().AddRangeAsync(auditEntries, cancellationToken);

        var result = await base.SaveChangesAsync(cancellationToken);

        // 4 — Clear domain events
        foreach (var entity in ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count != 0))
            entity.Entity.ClearDomainEvents();

        return result;
    }

    private List<AuditEntry> BuildAuditEntries()
    {
        var entries = new List<AuditEntry>();
        var userId = _currentUserService?.UserId?.ToString();
        var tenantId = _tenantProvider?.CurrentTenantId;

        foreach (var entry in ChangeTracker.Entries())
        {
            // Skip audit entries themselves and unchanged/detached entries
            if (entry.Entity is AuditEntry) continue;
            if (entry.Entity is not BaseEntity baseEntity) continue;

            AuditAction? action = entry.State switch
            {
                EntityState.Added    => AuditAction.Created,
                EntityState.Modified => AuditAction.Updated,
                EntityState.Deleted  => AuditAction.Deleted,
                _                    => null
            };

            if (action is null) continue;

            var oldValues = (action == AuditAction.Updated || action == AuditAction.Deleted)
                ? JsonSerializer.Serialize(entry.OriginalValues.Properties
                    .ToDictionary(p => p.Name, p => entry.OriginalValues[p]))
                : null;

            var newValues = (action == AuditAction.Created || action == AuditAction.Updated)
                ? JsonSerializer.Serialize(entry.CurrentValues.Properties
                    .ToDictionary(p => p.Name, p => entry.CurrentValues[p]))
                : null;

            entries.Add(new AuditEntry
            {
                EntityName = entry.Entity.GetType().Name,
                EntityId = baseEntity.Id.ToString(),
                Action = action.Value,
                OldValues = oldValues,
                NewValues = newValues,
                ChangedBy = userId,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow
            });
        }

        return entries;
    }

    private void ApplyGlobalFilters(ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            var isTenantScoped = typeof(ITenantScopedEntity).IsAssignableFrom(clrType);
            var isSoftDeletable = typeof(ISoftDeletable).IsAssignableFrom(clrType);

            if (!isTenantScoped && !isSoftDeletable) continue;

            var methodName = (isTenantScoped, isSoftDeletable) switch
            {
                (true,  true)  => nameof(SetTenantAndSoftDeleteFilter),
                (true,  false) => nameof(SetTenantFilter),
                (false, true)  => nameof(SetSoftDeleteFilter),
                _              => null!
            };

            GetType()
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(clrType)
                .Invoke(this, [builder]);
        }
    }

    private void SetTenantFilter<T>(ModelBuilder builder) where T : class, ITenantScopedEntity
    {
        builder.Entity<T>().HasQueryFilter(e =>
            _tenantProvider == null
            || _tenantProvider.CurrentTenantId == null
            || e.TenantId == _tenantProvider.CurrentTenantId.Value);
    }

    private void SetSoftDeleteFilter<T>(ModelBuilder builder) where T : class, ISoftDeletable
    {
        builder.Entity<T>().HasQueryFilter(e => e.DeletedAt == null);
    }

    private void SetTenantAndSoftDeleteFilter<T>(ModelBuilder builder)
        where T : class, ITenantScopedEntity, ISoftDeletable
    {
        builder.Entity<T>().HasQueryFilter(e =>
            e.DeletedAt == null
            && (_tenantProvider == null
                || _tenantProvider.CurrentTenantId == null
                || e.TenantId == _tenantProvider.CurrentTenantId.Value));
    }
}
