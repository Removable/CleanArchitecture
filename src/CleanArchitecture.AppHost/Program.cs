using Projects;

var builder = DistributedApplication.CreateBuilder(args);

const string databaseName = "CleanArchitectureDb";

var postgres = builder
    .AddPostgres("postgres")
    .WithEnvironment("POSTGRES_DB", databaseName);

var database = postgres.AddDatabase(databaseName);

builder.AddProject<CleanArchitecture_Web>("web")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
