using Asp.Versioning;
using Ecommerce.OrderService.Api;
using Ecommerce.OrderService.Application;
using Ecommerce.OrderService.Domain;
using Ecommerce.OrderService.Infrastructure.Messaging.Outbox;
using Ecommerce.OrderService.Infrastructure.Persistence;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ecommerce.OrderService.Api.Tests;

public sealed class OrderApiTests
{
    [Fact]
    public async Task GetOrder_ReturnsPersistedOrder()
    {
        await using var host = await OrderApiTestHost.CreateAsync();
        var orderId = await host.SeedOrderAsync();

        var response = await host.Client.GetAsync($"/api/v1/orders/{orderId}");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadAsStringAsync();
        payload.Should().Contain(orderId.ToString());
        payload.Should().Contain("Pending");
    }

    [Fact]
    public async Task GetOrder_WhenMissing_ReturnsProblemDetails()
    {
        await using var host = await OrderApiTestHost.CreateAsync();

        var response = await host.Client.GetAsync($"/api/v1/orders/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        var payload = await response.Content.ReadAsStringAsync();
        payload.Should().Contain("Order not found");
    }
}

internal sealed class OrderApiTestHost : IAsyncDisposable
{
    private readonly WebApplication _application;

    private OrderApiTestHost(WebApplication application, HttpClient client)
    {
        _application = application;
        Client = client;
    }

    public HttpClient Client { get; }

    public static async Task<OrderApiTestHost> CreateAsync()
    {
        var databaseRoot = new InMemoryDatabaseRoot();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddValidatorsFromAssembly(typeof(GlobalExceptionHandler).Assembly);
        builder.Services.AddOrderApplication();
        builder.Services.AddSingleton<IDomainEventOutboxMessageFactory, NullDomainEventOutboxMessageFactory>();
        builder.Services.AddSingleton(databaseRoot);
        builder.Services.AddDbContext<OrderDbContext>((serviceProvider, options) =>
            options.UseInMemoryDatabase(
                "order-api-tests",
                serviceProvider.GetRequiredService<InMemoryDatabaseRoot>()));
        builder.Services.AddScoped<IOrderDbContext>(serviceProvider => serviceProvider.GetRequiredService<OrderDbContext>());
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });

        var app = builder.Build();
        app.UseExceptionHandler();
        app.MapOrderEndpoints();
        await app.StartAsync();

        return new OrderApiTestHost(app, app.GetTestClient());
    }

    public async Task<Guid> SeedOrderAsync()
    {
        await using var scope = _application.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var order = Order.Create(
            Guid.NewGuid(),
            [new CreateOrderItemData(Guid.NewGuid(), "Keyboard", 2, 25m)],
            50m,
            Guid.NewGuid(),
            Guid.NewGuid());
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();
        return order.Id;
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
