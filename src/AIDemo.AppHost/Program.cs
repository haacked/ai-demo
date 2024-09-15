using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres")
    .WithPgAdmin()
    .WithDataVolume()
    .WithImage("ankane/pgvector")
    .WithDockerfile("docker/postgres")
    .WithBuildArg("VERSION", "14-3.2");

var postgresdb = postgres.AddDatabase("postgresdb");

var cache = builder
    .AddRedis("message-cache")
    .WithDataVolume()
    .WithPersistence(TimeSpan.FromSeconds(10));

builder.AddProject<AIDemo_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(postgresdb)
    .WithReference(cache);

if (!builder.ExecutionContext.IsPublishMode)
{
    // This is only for development.
    builder.AddExecutable(
        name: "ngrok",
        command: "ngrok",
        workingDirectory: ".",
        "tunnel", "--label", "edge=edghts_2UcgMuXxfrdN8OEN1oPK8W4kAB7", "https://localhost:7047");
}

builder.Build().Run();
