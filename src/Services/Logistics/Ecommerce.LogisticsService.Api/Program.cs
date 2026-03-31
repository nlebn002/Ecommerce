using Asp.Versioning;
using Ecommerce.LogisticsService.Api;
using Ecommerce.LogisticsService.Application;
using Ecommerce.LogisticsService.Infrastructure.DependencyInjection;
using Ecommerce.LogisticsService.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<AssemblyReference>();
builder.Services.AddLogisticsApplication();
builder.Services.AddLogisticsInfrastructure(builder.Configuration);
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

var app = builder.Build();

await ApplyMigrationsAsync<LogisticsDbContext>(app.Services);

app.UseExceptionHandler();
app.MapOpenApi();
app.MapScalarApiReference("/scalar");
app.MapGet("/", () => Results.Ok(new { service = "logistics-api", status = "ok" }));
app.MapLogisticsEndpoints();
app.MapDefaultEndpoints();

app.Run();

static async Task ApplyMigrationsAsync<TDbContext>(IServiceProvider services)
    where TDbContext : DbContext
{
    await using var scope = services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

    await dbContext.Database.MigrateAsync();
}
