using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var postgresdb = postgres.AddDatabase("postgresdb");

builder.AddProject<Projects.AIDemo_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(postgresdb);

builder.Build().Run();
