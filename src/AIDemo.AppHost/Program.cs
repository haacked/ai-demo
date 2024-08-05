using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres")
    .WithImage("ankane/pgvector")
    .WithDockerfile("docker/postgres")
    .WithBuildArg("VERSION", "14-3.2");

var postgresdb = postgres.AddDatabase("postgresdb");

builder.AddProject<AIDemo_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(postgresdb);

builder.Build().Run();
