var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Ecommerce_Basket_Api>("basket-api");
builder.AddProject<Projects.Ecommerce_Order_Api>("order-api");
builder.AddProject<Projects.Ecommerce_Logistics_Api>("logistics-api");
builder.AddProject<Projects.Ecommerce_Gateway>("gateway")
    .WithExternalHttpEndpoints();

builder.Build().Run();
