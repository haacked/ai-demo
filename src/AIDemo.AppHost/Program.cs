using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres")
    .WithImage("ankane/pgvector");

var postgresdb = postgres.AddDatabase("postgresdb");

builder.AddProject<AIDemo_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(postgresdb);

builder.Build().Run();
