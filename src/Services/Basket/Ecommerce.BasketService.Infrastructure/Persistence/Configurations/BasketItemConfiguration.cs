using Ecommerce.BasketService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.BasketService.Infrastructure.Persistence.Configurations;

internal sealed class BasketItemConfiguration : IEntityTypeConfiguration<BasketItem>
{
    public void Configure(EntityTypeBuilder<BasketItem> builder)
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .ValueGeneratedNever();

        builder.Property(entity => entity.BasketId)
            .IsRequired();

        builder.Property(entity => entity.ProductId)
            .IsRequired();

        builder.Property(entity => entity.ProductName)
            .IsRequired();

        builder.Property(entity => entity.UnitPrice)
            .HasPrecision(18, 2);

    }
}
