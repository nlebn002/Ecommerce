using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Forwarder;

var builder = WebApplication.CreateBuilder(args);
var serviceOpenApiEndpoints = builder.Configuration.GetSection("ServiceOpenApiEndpoints");
var frontendDevServerBaseUrl = builder.Configuration["FrontendDevServer:BaseUrl"] ?? "http://127.0.0.1:4200";

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddHttpForwarder();
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
builder.Services.AddSingleton(new ForwarderRequestConfig
{
    ActivityTimeout = TimeSpan.FromSeconds(30)
});
builder.Services.AddSingleton<HttpMessageInvoker>(_ => new HttpMessageInvoker(new SocketsHttpHandler
{
    UseCookies = false,
    UseProxy = false,
    AllowAutoRedirect = false,
    AutomaticDecompression = DecompressionMethods.None
}));
builder.Services.AddSingleton(new FrontendDevServerProbe(frontendDevServerBaseUrl));

var app = builder.Build();

app.UseExceptionHandler();
app.UseRateLimiter();
app.Map("/console", consoleApp =>
{
    consoleApp.Run(async context =>
    {
        var probe = context.RequestServices.GetRequiredService<FrontendDevServerProbe>();
        if (!await probe.IsAvailableAsync(context.RequestAborted))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var forwarder = context.RequestServices.GetRequiredService<IHttpForwarder>();
        var httpClient = context.RequestServices.GetRequiredService<HttpMessageInvoker>();
        var requestConfig = context.RequestServices.GetRequiredService<ForwarderRequestConfig>();
        var error = await forwarder.SendAsync(context, frontendDevServerBaseUrl, httpClient, requestConfig);

        if (error == ForwarderError.None)
        {
            return;
        }

        var errorFeature = context.GetForwarderErrorFeature();
        app.Logger.LogWarning(errorFeature?.Exception, "Angular dev server proxy failed. Falling back to static console files.");

        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    });
});
app.UseStaticFiles();
app.MapOpenApi();
app.MapGet("/console", () => Results.Redirect("/console/", permanent: false));
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
app.MapFallbackToFile("/console/{*path:nonfile}", "console/index.html");
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

sealed class FrontendDevServerProbe(string baseUrl)
{
    private readonly string _healthUrl = $"{baseUrl.TrimEnd('/')}/console/";
    private readonly Lock _lock = new();
    private DateTimeOffset _checkedAt = DateTimeOffset.MinValue;
    private bool _isAvailable;

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        lock (_lock)
        {
            if (now - _checkedAt < TimeSpan.FromSeconds(2))
            {
                return _isAvailable;
            }
        }

        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(400)
        };

        var available = false;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, _healthUrl);
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            available = (int)response.StatusCode < 500;
        }
        catch
        {
            available = false;
        }

        lock (_lock)
        {
            _checkedAt = now;
            _isAvailable = available;
        }

        return available;
    }
}
