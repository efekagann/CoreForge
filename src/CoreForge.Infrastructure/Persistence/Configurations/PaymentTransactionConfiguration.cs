using CoreForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreForge.Infrastructure.Persistence.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("PaymentTransactions");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Amount).HasPrecision(18, 2);
        builder.Property(t => t.Currency).HasMaxLength(3).IsRequired();
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(t => t.Plan).HasConversion<string>().HasMaxLength(50);
        builder.Property(t => t.ExternalTransactionId).HasMaxLength(255);
        builder.Property(t => t.Description).HasMaxLength(500);
        builder.HasIndex(t => t.TenantId);
    }
}
