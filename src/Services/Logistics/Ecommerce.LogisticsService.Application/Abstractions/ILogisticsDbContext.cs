using Ecommerce.LogisticsService.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.LogisticsService.Application;

public interface ILogisticsDbContext
{
    DbSet<Shipment> Shipments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
