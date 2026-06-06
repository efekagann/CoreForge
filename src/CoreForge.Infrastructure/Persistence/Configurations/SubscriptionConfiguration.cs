using CoreForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreForge.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Plan).HasConversion<string>().HasMaxLength(50);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(s => s.ExternalSubscriptionId).HasMaxLength(255);
        builder.HasIndex(s => s.TenantId);
    }
}
