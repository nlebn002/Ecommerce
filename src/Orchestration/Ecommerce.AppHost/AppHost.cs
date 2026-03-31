var builder = DistributedApplication.CreateBuilder(args);

var postgresUserName = builder.AddParameter("postgres-username", "postgres");
var postgresPassword = builder.AddParameter("postgres-password", "postgres", secret: true);
var postgres = builder.AddPostgres("postgres", postgresUserName, postgresPassword, 5432)
    .WithDataVolume("postgres-data");

var basketDb = postgres.AddDatabase("BasketDb", "basketdb");
var orderDb = postgres.AddDatabase("OrderDb", "orderdb");
var logisticsDb = postgres.AddDatabase("LogisticsDb", "logisticsdb");

var redis = builder.AddRedis("Redis");
var messageBroker = builder.AddRabbitMQ("MessageBroker");

var basketApi = builder.AddProject<Projects.Ecommerce_BasketService_Api>("basket-api")
    .WithReference(basketDb)
    .WithReference(redis)
    .WithReference(messageBroker)
    .WaitFor(basketDb)
    .WaitFor(redis)
    .WaitFor(messageBroker);

var orderApi = builder.AddProject<Projects.Ecommerce_OrderService_Api>("order-api")
    .WithReference(orderDb)
    .WithReference(messageBroker)
    .WaitFor(orderDb)
    .WaitFor(messageBroker);

var logisticsApi = builder.AddProject<Projects.Ecommerce_LogisticsService_Api>("logistics-api")
    .WithReference(logisticsDb)
    .WithReference(messageBroker)
    .WaitFor(logisticsDb)
    .WaitFor(messageBroker);

builder.AddProject<Projects.Ecommerce_Gateway>("gateway")
    .WithReference(basketApi)
    .WithReference(orderApi)
    .WithReference(logisticsApi)
    .WithExternalHttpEndpoints();

builder.Build().Run();
