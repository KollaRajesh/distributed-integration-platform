using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var sqlServer = builder.AddSqlServer("sqlserver");
var rabbitMq = builder.AddRabbitMQ("rabbitmq");
var keycloak = builder.AddContainer("keycloak", "quay.io/keycloak/keycloak:26.3")
    .WithHttpEndpoint(targetPort: 8080, name: "http")
    .WithEnvironment("KC_DB", "dev-file")
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin");

builder.AddProject<Projects.CustomerApi>("customer-api")
    .WithReference(postgres)
    .WithReference(rabbitMq)
    .WaitFor(keycloak);
builder.AddProject<Projects.ContractApi>("contract-api")
    .WithReference(postgres)
    .WithReference(rabbitMq)
    .WaitFor(keycloak);
builder.AddProject<Projects.InvoiceApi>("invoice-api")
    .WithReference(sqlServer)
    .WithReference(rabbitMq)
    .WaitFor(keycloak);
builder.AddProject<Projects.LedgerApi>("ledger-api")
    .WithReference(postgres)
    .WithReference(rabbitMq)
    .WaitFor(keycloak);
builder.AddProject<Projects.NotificationApi>("notification-api")
    .WithReference(postgres)
    .WithReference(rabbitMq)
    .WaitFor(keycloak);

builder.Build().Run();
