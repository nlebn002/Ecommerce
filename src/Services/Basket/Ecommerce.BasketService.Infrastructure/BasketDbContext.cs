using Ecommerce.BasketService.Application;
using Ecommerce.BasketService.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.BasketService.Infrastructure;

public sealed class BasketDbContext : DbContext, IBasketDbContext
{
    public BasketDbContext(DbContextOptions<BasketDbContext> options)
        : base(options)
    {
    }

    public DbSet<Basket> Baskets => Set<Basket>();

    public DbSet<BasketItem> BasketItems => Set<BasketItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Basket>(basket =>
        {
            basket.ToTable("baskets");
            basket.HasKey(entity => entity.Id);
            basket.HasQueryFilter(entity => !entity.IsDeleted);

            basket.Property(entity => entity.Id).HasColumnName("basket_id").ValueGeneratedNever();
            basket.Property(entity => entity.CreatedDate).HasColumnName("created_date").IsRequired();
            basket.Property(entity => entity.IsDeleted).HasColumnName("is_deleted").IsRequired();
            basket.Property(entity => entity.CustomerId).HasColumnName("customer_id").IsRequired();
            basket.Property(entity => entity.Status).HasColumnName("status").HasConversion<string>().IsRequired();
            basket.Property(entity => entity.Total).HasColumnName("total").HasPrecision(18, 2);

            basket.HasMany(entity => entity.Items)
                .WithOne()
                .HasForeignKey(entity => entity.BasketId);
        });

        modelBuilder.Entity<BasketItem>(item =>
        {
            item.ToTable("basket_items");
            item.HasKey(entity => entity.Id);
            item.HasQueryFilter(entity => !entity.IsDeleted);

            item.Property(entity => entity.Id).HasColumnName("basket_item_id").ValueGeneratedNever();
            item.Property(entity => entity.BasketId).HasColumnName("basket_id").IsRequired();
            item.Property(entity => entity.CreatedDate).HasColumnName("created_date").IsRequired();
            item.Property(entity => entity.IsDeleted).HasColumnName("is_deleted").IsRequired();
            item.Property(entity => entity.ProductId).HasColumnName("product_id").IsRequired();
            item.Property(entity => entity.ProductName).HasColumnName("product_name").IsRequired();
            item.Property(entity => entity.Quantity).HasColumnName("quantity");
            item.Property(entity => entity.UnitPrice).HasColumnName("unit_price").HasPrecision(18, 2);

            item.HasIndex(entity => new { entity.BasketId, entity.ProductId }).IsUnique();
        });
    }
}

