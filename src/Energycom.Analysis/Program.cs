using Energycom.Analysis.Services;
using Energycom.Ingestion.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

builder.AddServiceDefaults();

builder.Services.AddHttpClient("ingestion", conf => {
    conf.BaseAddress = new Uri("https://Ingestion");
}).AddServiceDiscovery();

builder.AddNpgsqlDbContext<ECOMDbContext>("EnergycomDb", settings => {
    settings.DisableRetry = true;
    settings.CommandTimeout = 30;
}, options => {
    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
    options.UseSnakeCaseNamingConvention();
});

// Register Services
builder.Services.AddScoped<IEnergyAnalysisService, EnergyAnalysisService>();
// Register repositories
builder.Services.AddScoped<IReadingRepository, ReadingRepository>();

builder.Services.AddHostedService<ConsoleApp>();

var host = builder.Build();
await host.RunAsync();
