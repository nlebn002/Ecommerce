using Asp.Versioning;
using Ecommerce.Common.Messaging.Outbox;
using Ecommerce.LogisticsService.Api;
using Ecommerce.LogisticsService.Application;
using Ecommerce.LogisticsService.Domain;
using Ecommerce.LogisticsService.Infrastructure.Messaging.Outbox;
using Ecommerce.LogisticsService.Infrastructure.Persistence;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ecommerce.LogisticsService.Api.Tests;

public sealed class LogisticsApiTests
{
    [Fact]
    public async Task GetShipment_ReturnsPersistedShipment()
    {
        await using var host = await LogisticsApiTestHost.CreateAsync();
        var shipmentId = await host.SeedShipmentAsync();

        var response = await host.Client.GetAsync($"/api/v1/shipments/{shipmentId}");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadAsStringAsync();
        payload.Should().Contain(shipmentId.ToString());
        payload.Should().Contain("Reserved");
    }

    [Fact]
    public async Task GetShipment_WhenMissing_ReturnsProblemDetails()
    {
        await using var host = await LogisticsApiTestHost.CreateAsync();

        var response = await host.Client.GetAsync($"/api/v1/shipments/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        var payload = await response.Content.ReadAsStringAsync();
        payload.Should().Contain("Shipment not found");
    }

    [Fact]
    public async Task GetShipmentByOrder_ReturnsPersistedShipment()
    {
        await using var host = await LogisticsApiTestHost.CreateAsync();
        var shipmentId = await host.SeedShipmentAsync();
        var orderId = await host.GetOrderIdAsync(shipmentId);

        var response = await host.Client.GetAsync($"/api/v1/shipments/by-order/{orderId}");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadAsStringAsync();
        payload.Should().Contain(orderId.ToString());
    }
}

internal sealed class LogisticsApiTestHost : IAsyncDisposable
{
    private readonly WebApplication _application;

    private LogisticsApiTestHost(WebApplication application, HttpClient client)
    {
        _application = application;
        Client = client;
    }

    public HttpClient Client { get; }

    public static async Task<LogisticsApiTestHost> CreateAsync()
    {
        var databaseRoot = new InMemoryDatabaseRoot();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddValidatorsFromAssembly(typeof(GlobalExceptionHandler).Assembly);
        builder.Services.AddLogisticsApplication();
        builder.Services.AddSingleton<IDomainEventOutboxMessageFactory, NullDomainEventOutboxMessageFactory>();
        builder.Services.AddSingleton(databaseRoot);
        builder.Services.AddDbContext<LogisticsDbContext>((serviceProvider, options) =>
            options.UseInMemoryDatabase(
                "logistics-api-tests",
                serviceProvider.GetRequiredService<InMemoryDatabaseRoot>()));
        builder.Services.AddScoped<ILogisticsDbContext>(serviceProvider => serviceProvider.GetRequiredService<LogisticsDbContext>());
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });

        var app = builder.Build();
        app.UseExceptionHandler();
        app.MapLogisticsEndpoints();
        await app.StartAsync();

        return new LogisticsApiTestHost(app, app.GetTestClient());
    }

    public async Task<Guid> SeedShipmentAsync()
    {
        await using var scope = _application.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LogisticsDbContext>();
        var shipment = Shipment.CreateForOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        dbContext.Shipments.Add(shipment);
        await dbContext.SaveChangesAsync();
        return shipment.Id;
    }

    public async Task<Guid> GetOrderIdAsync(Guid shipmentId)
    {
        await using var scope = _application.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LogisticsDbContext>();
        var shipment = await dbContext.Shipments.SingleAsync(entity => entity.Id == shipmentId);
        return shipment.OrderId;
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _application.DisposeAsync();
    }

    private sealed class NullDomainEventOutboxMessageFactory : IDomainEventOutboxMessageFactory
    {
        public IReadOnlyCollection<OutboxMessage> CreateMessages(IReadOnlyCollection<IDomainEvent> domainEvents) => [];
    }
}
