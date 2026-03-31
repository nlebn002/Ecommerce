using Ecommerce.Common.Messaging.Outbox;
using Ecommerce.LogisticsService.Application;
using Ecommerce.LogisticsService.Domain;
using Ecommerce.LogisticsService.Infrastructure.Messaging.Outbox;
using Ecommerce.LogisticsService.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.LogisticsService.Application.Tests;

public sealed class ShipmentHandlerContractTests
{
    [Fact]
    public async Task ReserveShipmentForOrderHandler_CreatesReservedShipment()
    {
        await using var dbContext = CreateDbContext(nameof(ReserveShipmentForOrderHandler_CreatesReservedShipment));
        var handler = new ReserveShipmentForOrderHandler(dbContext);

        var shipment = await handler.ExecuteAsync(
            new ReserveShipmentForOrderCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        shipment.Status.Should().Be(ShipmentStatus.Reserved.ToString());
        shipment.Carrier.Should().Be("DefaultCarrier");
        (await dbContext.Shipments.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task GetShipmentHandler_WhenMissing_ThrowsNotFound()
    {
        await using var dbContext = CreateDbContext(nameof(GetShipmentHandler_WhenMissing_ThrowsNotFound));
        var handler = new GetShipmentHandler(dbContext);

        var act = () => handler.ExecuteAsync(new GetShipmentQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<LogisticsException>()
            .Where(exception => exception.Code == LogisticsErrorCode.ShipmentNotFound && exception.Type == LogisticsErrorType.NotFound);
    }

    [Fact]
    public async Task GetShipmentByOrderHandler_ReturnsPersistedShipment()
    {
        await using var dbContext = CreateDbContext(nameof(GetShipmentByOrderHandler_ReturnsPersistedShipment));
        var shipment = Shipment.CreateForOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        shipment.ClearDomainEvents();
        dbContext.Shipments.Add(shipment);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var handler = new GetShipmentByOrderHandler(dbContext);

        var result = await handler.ExecuteAsync(new GetShipmentByOrderQuery(shipment.OrderId), CancellationToken.None);

        result.Id.Should().Be(shipment.Id);
        result.OrderId.Should().Be(shipment.OrderId);
    }

    [Fact]
    public async Task FailShipmentHandler_UpdatesShipmentStatus()
    {
        await using var dbContext = CreateDbContext(nameof(FailShipmentHandler_UpdatesShipmentStatus));
        var shipment = Shipment.CreateFailed(Guid.NewGuid(), "seed", Guid.NewGuid(), Guid.NewGuid());
        shipment.ClearDomainEvents();
        dbContext.Shipments.Add(shipment);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var handler = new GetShipmentHandler(dbContext);

        var result = await handler.ExecuteAsync(new GetShipmentQuery(shipment.Id), CancellationToken.None);

        result.Status.Should().Be(ShipmentStatus.Failed.ToString());
    }

    private static LogisticsDbContext CreateDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<LogisticsDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new LogisticsDbContext(options, new NullDomainEventOutboxMessageFactory());
    }

    private sealed class NullDomainEventOutboxMessageFactory : IDomainEventOutboxMessageFactory
    {
        public IReadOnlyCollection<OutboxMessage> CreateMessages(IReadOnlyCollection<IDomainEvent> domainEvents) => [];
    }
}
