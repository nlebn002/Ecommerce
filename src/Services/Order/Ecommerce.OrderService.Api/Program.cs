using Asp.Versioning;
using Ecommerce.OrderService.Api;
using Ecommerce.OrderService.Application;
using Ecommerce.OrderService.Infrastructure.DependencyInjection;
using Ecommerce.OrderService.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<AssemblyReference>();
builder.Services.AddOrderApplication();
builder.Services.AddOrderInfrastructure(builder.Configuration);
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

var app = builder.Build();

await ApplyMigrationsAsync<OrderDbContext>(app.Services);

app.UseExceptionHandler();
app.MapOpenApi();
app.MapScalarApiReference("/scalar");
app.MapGet("/", () => Results.Ok(new { service = "order-api", status = "ok" }));
app.MapOrderEndpoints();
app.MapDefaultEndpoints();

app.Run();

static async Task ApplyMigrationsAsync<TDbContext>(IServiceProvider services)
    where TDbContext : DbContext
{
    await using var scope = services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

    await dbContext.Database.MigrateAsync();
}
