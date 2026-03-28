using Microsoft.AspNetCore.RateLimiting;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("gateway", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromSeconds(10);
        limiterOptions.QueueLimit = 0;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

app.UseExceptionHandler();
app.UseRateLimiter();
app.MapOpenApi();
app.MapScalarApiReference("/scalar", options =>
{
    options.WithDynamicBaseServerUrl(true);
    options.AddDocument("gateway", "Gateway API", "/openapi/v1.json");
    options.AddDocument("basket-api", "Basket API", "/scalar-docs/basket/v1.json", true);
});
app.MapGet("/", () => Results.Ok(new { service = "gateway", status = "ok" }));
app.MapReverseProxy().RequireRateLimiting("gateway");
app.MapDefaultEndpoints();

app.Run();
