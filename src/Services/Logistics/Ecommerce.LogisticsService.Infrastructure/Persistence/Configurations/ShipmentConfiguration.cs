using Ecommerce.LogisticsService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.LogisticsService.Infrastructure.Persistence.Configurations;

internal sealed class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedNever();
        builder.Property(entity => entity.OrderId).IsRequired();
        builder.Property(entity => entity.Carrier).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.ShippingPrice).HasPrecision(18, 2);
        builder.Property(entity => entity.Status).HasConversion<string>().IsRequired();
        builder.Property(entity => entity.FailureReason).HasMaxLength(512);
        builder.HasIndex(entity => entity.OrderId).IsUnique();
    }
}
