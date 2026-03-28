using Asp.Versioning;
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
builder.Services.AddOpenApi();
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
app.MapGet("/api/basket", () => Results.Ok(new[] { "Basket service skeleton" }));
app.MapDefaultEndpoints();

app.Run();
