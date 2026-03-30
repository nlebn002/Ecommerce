using Ecommerce.OrderService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Ecommerce.OrderService.Infrastructure.Persistence.Interceptors;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly TimeProvider _timeProvider;

    public AuditSaveChangesInterceptor(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditFields(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        foreach (var entry in dbContext.ChangeTracker.Entries<Entity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(entity => entity.CreatedDate).CurrentValue = utcNow;
                    entry.Property(entity => entity.UpdatedDate).CurrentValue = null;
                    entry.Property(entity => entity.IsDeleted).CurrentValue = false;
                    SetConcurrencyToken(entry);
                    break;
                case EntityState.Modified:
                    entry.Property(entity => entity.CreatedDate).IsModified = false;
                    entry.Property(entity => entity.UpdatedDate).CurrentValue = utcNow;
                    SetConcurrencyToken(entry);
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Property(entity => entity.CreatedDate).IsModified = false;
                    entry.Property(entity => entity.IsDeleted).CurrentValue = true;
                    entry.Property(entity => entity.UpdatedDate).CurrentValue = utcNow;
                    SetConcurrencyToken(entry);
                    break;
            }
        }
    }

    private static void SetConcurrencyToken(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Entity> entry)
    {
        entry.Property(entity => entity.ConcurrencyToken).CurrentValue = Guid.NewGuid();
    }
}
