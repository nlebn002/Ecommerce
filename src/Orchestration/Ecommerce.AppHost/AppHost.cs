using k8s.Models;

var builder = DistributedApplication.CreateBuilder(args);

var postgresUserName = builder.AddParameter("postgres-username", "postgres");
var postgresPassword = builder.AddParameter("postgres-password", "postgres", secret: true);
var postgres = builder.AddPostgres("postgres", postgresUserName, postgresPassword, 5432)
    .WithDataBindMount(@"C:\Temp\databases\basket");

var basketDb = postgres.AddDatabase("BasketDb", "basketdb");

var redis = builder.AddRedis("Redis");
var messageBroker = builder.AddRabbitMQ("MessageBroker");

var basketApi = builder.AddProject<Projects.Ecommerce_BasketService_Api>("basket-api")
    .WithReference(basketDb)
    .WithReference(redis)
    .WithReference(messageBroker)
    .WaitFor(basketDb)
    .WaitFor(redis)
    .WaitFor(messageBroker);

builder.AddProject<Projects.Ecommerce_Gateway>("gateway")
    .WithReference(basketApi)
    .WithExternalHttpEndpoints();

builder.Build().Run();
