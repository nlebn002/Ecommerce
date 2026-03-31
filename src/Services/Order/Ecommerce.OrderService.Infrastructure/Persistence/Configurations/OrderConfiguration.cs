using Ecommerce.OrderService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.OrderService.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedNever();
        builder.Property(entity => entity.CustomerId).IsRequired();
        builder.Property(entity => entity.CheckoutCorrelationId).IsRequired();
        builder.Property(entity => entity.Status).HasConversion<string>().IsRequired();
        builder.Property(entity => entity.ItemsTotal).HasPrecision(18, 2);
        builder.Property(entity => entity.ShippingPrice).HasPrecision(18, 2);
        builder.Property(entity => entity.FinalTotal).HasPrecision(18, 2);
        builder.Property(entity => entity.CancellationReason).HasMaxLength(512);
        builder.HasIndex(entity => entity.CheckoutCorrelationId).IsUnique();
        builder.HasMany(entity => entity.Items).WithOne().HasForeignKey(entity => entity.OrderId);
    }
}
