using Ecommerce.Basket.Application;
using Ecommerce.Basket.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Basket.Infrastructure;

public sealed class BasketDbContext : DbContext, IBasketDbContext
{
    public BasketDbContext(DbContextOptions<BasketDbContext> options)
        : base(options)
    {
    }

    public DbSet<Basket> Baskets => Set<Basket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Basket>(basket =>
        {
            basket.ToTable("baskets");
            basket.HasKey(entity => entity.Id);

            basket.Property(entity => entity.Id).HasColumnName("basket_id");
            basket.Property(entity => entity.CustomerId).HasColumnName("customer_id").IsRequired();
            basket.Property(entity => entity.Status).HasColumnName("status").HasConversion<string>().IsRequired();
            basket.Property(entity => entity.Total).HasColumnName("total").HasPrecision(18, 2);

            basket.OwnsMany(entity => entity.Items, item =>
            {
                item.ToTable("basket_items");
                item.WithOwner().HasForeignKey("basket_id");
                item.Property<string>("basket_id");
                item.HasKey("basket_id", nameof(BasketItem.ProductId));

                item.Property(entity => entity.ProductId).HasColumnName("product_id");
                item.Property(entity => entity.ProductName).HasColumnName("product_name").IsRequired();
                item.Property(entity => entity.Quantity).HasColumnName("quantity");
                item.Property(entity => entity.UnitPrice).HasColumnName("unit_price").HasPrecision(18, 2);
            });
        });
    }
}
