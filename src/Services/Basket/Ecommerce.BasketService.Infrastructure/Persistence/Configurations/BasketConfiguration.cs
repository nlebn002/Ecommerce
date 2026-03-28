using Ecommerce.BasketService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.BasketService.Infrastructure.Persistence.Configurations;

internal sealed class BasketConfiguration : IEntityTypeConfiguration<Basket>
{
    public void Configure(EntityTypeBuilder<Basket> builder)
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .ValueGeneratedNever();

        builder.Property(entity => entity.CustomerId)
            .IsRequired();

        builder.Property(entity => entity.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(entity => entity.Total)
            .HasPrecision(18, 2);

        builder.HasMany(entity => entity.Items)
            .WithOne()
            .HasForeignKey(entity => entity.BasketId);
    }
}
