var builder = DistributedApplication.CreateBuilder(args);

// --- Infrastructure ---

// PostgreSQL with pgAdmin
var postgresPassword = builder.AddParameter("postgres-password", secret: true);
var postgres = builder.AddPostgres("postgres", password: postgresPassword)
    .WithDataVolume()
    .WithPgAdmin();

var nutrify = postgres.AddDatabase("nutrify");

// Redis (optional caching)
var redis = builder.AddRedis("redis");

// Keycloak identity provider
var keycloak = builder.AddKeycloakContainer("keycloak", port: 8080)
    .WithDataVolume()
    .WithImport("./Realms");

var realm = keycloak.AddRealm("nutrify-realm", "nutrify");

// --- Application Services ---

// API
var api = builder.AddProject<Projects.Nutrify_Api>("api")
    .WithReference(nutrify)
    .WaitFor(nutrify)
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(realm)
    .WaitFor(keycloak)
    .WithHttpHealthCheck("/health");

// React Frontend (Vite)
builder.AddViteApp("client", "../Nutrify.Client")
    .WithNpm()
    .WithReference(api)
    .WaitFor(api)
    .WaitFor(keycloak)
    .WithExternalHttpEndpoints();

builder.Build().Run();
