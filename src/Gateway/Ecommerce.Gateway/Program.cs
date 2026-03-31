using Microsoft.AspNetCore.RateLimiting;
using Scalar.AspNetCore;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
var serviceOpenApiEndpoints = builder.Configuration.GetSection("ServiceOpenApiEndpoints");

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
app.MapGet("/scalar-docs/order/{documentName}.json", (
    string documentName,
    IHttpClientFactory httpClientFactory,
    CancellationToken cancellationToken) =>
    GetServiceOpenApiDocumentAsync(
        "order-api",
        "/orders/",
        "Order",
        documentName,
        httpClientFactory,
        serviceOpenApiEndpoints,
        cancellationToken));
app.MapGet("/scalar-docs/logistics/{documentName}.json", (
    string documentName,
    IHttpClientFactory httpClientFactory,
    CancellationToken cancellationToken) =>
    GetServiceOpenApiDocumentAsync(
        "logistics-api",
        "/logistics/",
        "Logistics",
        documentName,
        httpClientFactory,
        serviceOpenApiEndpoints,
        cancellationToken));
app.MapGet("/scalar-docs/basket/{documentName}.json", (
    string documentName,
    IHttpClientFactory httpClientFactory,
    CancellationToken cancellationToken) =>
    GetServiceOpenApiDocumentAsync(
        "basket-api",
        "/basket/",
        "Basket",
        documentName,
        httpClientFactory,
        serviceOpenApiEndpoints,
        cancellationToken));
app.MapScalarApiReference("/scalar", options =>
{
    options.WithDynamicBaseServerUrl(true);
    options.AddDocument("gateway", "Gateway API", "/openapi/v1.json");
    options.AddDocument("basket-api", "Basket API", "/scalar-docs/basket/v1.json", true);
    options.AddDocument("order-api", "Order API", "/scalar-docs/order/v1.json", true);
    options.AddDocument("logistics-api", "Logistics API", "/scalar-docs/logistics/v1.json", true);
});
app.MapGet("/", () => Results.Ok(new { service = "gateway", status = "ok" }));
app.MapReverseProxy().RequireRateLimiting("gateway");
app.MapDefaultEndpoints();

app.Run();

static async Task<IResult> GetServiceOpenApiDocumentAsync(
    string serviceName,
    string gatewayPrefix,
    string displayName,
    string documentName,
    IHttpClientFactory httpClientFactory,
    IConfigurationSection serviceOpenApiEndpoints,
    CancellationToken cancellationToken)
{
    var serviceBaseAddress = serviceOpenApiEndpoints[serviceName] ?? $"https+http://{serviceName}";

    if (!Uri.TryCreate(serviceBaseAddress, UriKind.Absolute, out var serviceBaseUri))
    {
        return Results.Problem($"{displayName} OpenAPI endpoint '{serviceBaseAddress}' is not a valid absolute URI.");
    }

    var openApiUri = new Uri(serviceBaseUri, $"openapi/{documentName}.json");
    var client = httpClientFactory.CreateClient();
    var response = await client.GetAsync(openApiUri, cancellationToken);
    if (!response.IsSuccessStatusCode)
    {
        return Results.StatusCode((int)response.StatusCode);
    }

    await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
    var document = await JsonNode.ParseAsync(responseStream, cancellationToken: cancellationToken);
    if (document is not JsonObject root)
    {
        return Results.Problem($"{displayName} OpenAPI document was not valid JSON.");
    }

    if (root["paths"] is JsonObject paths)
    {
        var rewrittenPaths = new JsonObject();
        foreach (var path in paths)
        {
            var rewrittenPath = path.Key.StartsWith("/api/", StringComparison.Ordinal)
                ? $"{gatewayPrefix}{path.Key["/api/".Length..]}"
                : path.Key;
            rewrittenPaths[rewrittenPath] = path.Value?.DeepClone();
        }

        root["paths"] = rewrittenPaths;
    }

    root["servers"] = new JsonArray(new JsonObject
    {
        ["url"] = "/"
    });

    return Results.Text(root.ToJsonString(new JsonSerializerOptions(JsonSerializerDefaults.Web)), "application/json");
}
