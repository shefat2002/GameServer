var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var username = builder.AddParameter("keycloak-username", "admin");
var password = builder.AddParameter("keycloak-password", "admin", secret:true);

var keycloak = builder.AddKeycloak("keycloak", 8080, username, password)
    .WithDataVolume();

var apiService = builder.AddProject<Projects.GameServer_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(cache);

builder.AddProject<Projects.GameServer_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithReference(keycloak)
    .WaitFor(keycloak);

builder.Build().Run();