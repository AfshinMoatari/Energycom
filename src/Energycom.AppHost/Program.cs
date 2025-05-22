using Energycom.AppHost;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("EnergycomDbServer")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin(opts => opts.WithHostPort(23371));


IResourceBuilder<PostgresDatabaseResource> db = postgres.AddDatabase("EnergycomDb").WithCreateCommand(true);

var backend = builder.AddProject<Energycom_Ingestion>("Ingestion")
    .WithReference(db)
    .WaitFor(db)
    .WithDashboardResource("scalar", "scalar", "scalar");

var analysis = builder.AddProject<Energycom_Analysis>("Analysis")
    .WithReference(db)
    .WithReference(backend)
    .WaitFor(db)
    .WaitFor(backend);

builder.Build().Run();