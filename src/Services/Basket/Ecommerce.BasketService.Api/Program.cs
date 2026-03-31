using Asp.Versioning;
using Ecommerce.BasketService.Api;
using Ecommerce.BasketService.Application;
using Ecommerce.BasketService.Infrastructure.Persistence;
using Ecommerce.BasketService.Infrastructure.DependencyInjection;
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
builder.Services.AddBasketApplication();
builder.Services.AddBasketInfrastructure(builder.Configuration);
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

var app = builder.Build();

await ApplyMigrationsAsync<BasketDbContext>(app.Services);

app.UseExceptionHandler();
app.MapOpenApi();
app.MapScalarApiReference("/scalar");
app.MapGet("/", () => Results.Ok(new { service = "basket-api", status = "ok" }));
app.MapBasketEndpoints();
app.MapDefaultEndpoints();

app.Run();

static async Task ApplyMigrationsAsync<TDbContext>(IServiceProvider services)
    where TDbContext : DbContext
{
    await using var scope = services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

    await dbContext.Database.MigrateAsync();
}

