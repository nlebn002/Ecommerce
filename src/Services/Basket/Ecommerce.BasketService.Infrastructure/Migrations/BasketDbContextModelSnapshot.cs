using System;
using Ecommerce.BasketService.Domain;
using Ecommerce.BasketService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Ecommerce.BasketService.Infrastructure.Migrations;

[DbContext(typeof(BasketDbContext))]
partial class BasketDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity<Basket>(b =>
        {
            b.Property(entity => entity.Id)
                .ValueGeneratedNever()
                .HasColumnType("uuid");

            b.Property(entity => entity.ConcurrencyToken)
                .IsConcurrencyToken()
                .HasColumnType("uuid");

            b.Property(entity => entity.CreatedDate)
                .HasColumnType("timestamp with time zone");

            b.Property(entity => entity.CustomerId)
                .IsRequired()
                .HasColumnType("uuid");

            b.Property(entity => entity.IsDeleted)
                .HasColumnType("boolean");

            b.Property(entity => entity.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasColumnType("text");

            b.Property(entity => entity.Total)
                .HasPrecision(18, 2)
                .HasColumnType("numeric(18,2)");

            b.Property(entity => entity.UpdatedDate)
                .HasColumnType("timestamp with time zone");

            b.HasKey(entity => entity.Id);

            b.HasQueryFilter(entity => !entity.IsDeleted);

            b.ToTable("Baskets");
        });

        modelBuilder.Entity<BasketItem>(b =>
        {
            b.Property(entity => entity.Id)
                .ValueGeneratedNever()
                .HasColumnType("uuid");

            b.Property(entity => entity.BasketId)
                .HasColumnType("uuid");

            b.Property(entity => entity.ConcurrencyToken)
                .IsConcurrencyToken()
                .HasColumnType("uuid");

            b.Property(entity => entity.CreatedDate)
                .HasColumnType("timestamp with time zone");

            b.Property(entity => entity.IsDeleted)
                .HasColumnType("boolean");

            b.Property(entity => entity.ProductId)
                .IsRequired()
                .HasColumnType("uuid");

            b.Property(entity => entity.ProductName)
                .IsRequired()
                .HasColumnType("text");

            b.Property(entity => entity.Quantity)
                .HasColumnType("integer");

            b.Property(entity => entity.UnitPrice)
                .HasPrecision(18, 2)
                .HasColumnType("numeric(18,2)");

            b.Property(entity => entity.UpdatedDate)
                .HasColumnType("timestamp with time zone");

            b.HasKey(entity => entity.Id);

            b.HasIndex(entity => entity.BasketId);

            b.HasQueryFilter(entity => !entity.IsDeleted);

            b.ToTable("BasketItems");
        });

        modelBuilder.Entity<BasketItem>(b =>
        {
            b.HasOne<Basket>()
                .WithMany(entity => entity.Items)
                .HasForeignKey(entity => entity.BasketId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });
#pragma warning restore 612, 618
    }
}
