var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Ecommerce_BasketService_Api>("basket-api");
builder.AddProject<Projects.Ecommerce_OrderService_Api>("order-api");
builder.AddProject<Projects.Ecommerce_LogisticsService_Api>("logistics-api");
builder.AddProject<Projects.Ecommerce_Gateway>("gateway")
    .WithExternalHttpEndpoints();

builder.Build().Run();
