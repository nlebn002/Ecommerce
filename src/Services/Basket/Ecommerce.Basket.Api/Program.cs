using Asp.Versioning;
using Ecommerce.Basket.Api;
using Ecommerce.Basket.Application;
using Ecommerce.Basket.Infrastructure;
using FluentValidation;
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
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddBasketApplication();
builder.Services.AddBasketInfrastructure(builder.Configuration);
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

var app = builder.Build();

app.UseExceptionHandler();
app.MapOpenApi();
app.MapScalarApiReference("/scalar");
app.MapGet("/", () => Results.Ok(new { service = "basket-api", status = "ok" }));
app.MapBasketEndpoints();
app.MapDefaultEndpoints();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BasketDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();
