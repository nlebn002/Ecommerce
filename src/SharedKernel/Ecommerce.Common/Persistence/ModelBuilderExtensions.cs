using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Common.Persistence;

public static class ModelBuilderExtensions
{
    public static void ApplyAuditPropertyConventions(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(Entity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            modelBuilder.Entity(entityType.ClrType).Property(nameof(Entity.CreatedDate)).IsRequired();
            modelBuilder.Entity(entityType.ClrType).Property(nameof(Entity.UpdatedDate));
            modelBuilder.Entity(entityType.ClrType).Property(nameof(Entity.IsDeleted)).IsRequired();
            modelBuilder.Entity(entityType.ClrType).Property(nameof(Entity.ConcurrencyToken)).IsConcurrencyToken();
        }
    }

    public static void ApplySoftDeleteQueryFilters(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(Entity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "entity");
            var isDeleted = Expression.Property(parameter, nameof(Entity.IsDeleted));
            var compare = Expression.Equal(isDeleted, Expression.Constant(false));
            var lambda = Expression.Lambda(compare, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}
