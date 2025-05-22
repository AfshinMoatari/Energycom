using System.Runtime.CompilerServices;
using Energycom.Ingestion;
using Energycom.Ingestion.Data;
using Energycom.Ingestion.Entities;
using Energycom.Ingestion.Ingest;
using Energycom.Ingestion.Models;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddOpenApi();



builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
    logging.CombineLogs = true;
});

builder.AddNpgsqlDbContext<ECOMDbContext>("EnergycomDb", settings =>
{
    settings.DisableRetry = true;
    settings.CommandTimeout = 30;

}, options =>
{
    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
    options.UseSnakeCaseNamingConvention();
});

//This is a dumb way to do this, but is fine for this limited use case
#pragma warning disable ASP0000
if (!IsDesignTime())
{
    await MigrateAndSeed.Execute(builder.Services.BuildServiceProvider()); 
}
#pragma warning restore ASP0000


builder.Services.AddHostedService<TimedHostedService>();
builder.Services.AddSingleton<NotificationHub>();

var app = builder.Build();
app.MapDefaultEndpoints();
app.MapOpenApi();
app.MapScalarApiReference($"/scalar", options =>
{
    options
        .WithTitle("Energycom Ingestion")
        .WithTheme(ScalarTheme.Kepler)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);


    //hack for aspire url passthrough
    options.Servers = [];
});

app.UseHttpsRedirection();

app.MapGet("", () => Results.Redirect("/scalar", true));

app.MapGet("/sites", async ([FromServices] ECOMDbContext context) =>
{
    var sites = await context.Sites
        .ToListAsync();

    return Results.Ok(sites.Select(MapToSiteModel).ToList());
}).WithName("GetSites").WithTags("Get all sites");

app.MapGet("/groups", async ([FromServices] ECOMDbContext context) =>
{
    var groups = await context.Groups
        .ToListAsync();

    return Results.Ok(groups.Select(MapToGroupModel).ToList());
}).WithName("GetGroups").WithTags("Get all groups");


app.MapGet("/readings/stream", 
        ( [FromServices] NotificationHub hub, CancellationToken token,  [FromServices] ILogger<Program> logger) 
        => GetDataWithCancellationAsync(hub,logger, token ))
    .WithName("GetReadingsStream").WithTags("Get Readings");

app.MapGet("/readings/{from:datetime}/{to:datetime}", async (DateTime from, DateTime to, [FromServices] ECOMDbContext context ) =>
{
    var readings = await context.Readings
        .Include(reading => reading.Meter)
        .ThenInclude(meter => meter.Group)
        .Include(reading => reading.Meter)
        .ThenInclude(meter => meter.Site)
        .Where(e => e.IngestionDate >= from && e.IngestionDate < to)
        .ToListAsync();

    return Results.Ok(readings.Select(MapToReadingModel).ToList());
}).WithName("GetReadings").WithTags("Get Readings").WithDescription("Get readings between from(inclusive) and to(exclusive)");

app.MapPost("/readings/reset", async ([FromServices] ECOMDbContext context) =>
{
    var readings = await context.Readings.ToListAsync();
    context.RemoveRange(readings);
    await context.SaveChangesAsync();
    return Results.Ok("Reset");
}).WithName("ResetReadings").WithTags("Delete all readings");




app.Run();

async IAsyncEnumerable<ReadingModelV1> GetDataWithCancellationAsync(NotificationHub hub, ILogger logger,  [EnumeratorCancellation] CancellationToken cancellationToken = default)
{

    var channel = await hub.Subscribe();
    cancellationToken.Register(() =>
    {
        logger.LogInformation("Cancellation requested, stopping stream.");
        hub.Unsubscribe(channel);
    });
    while (!cancellationToken.IsCancellationRequested)
    { 
        await foreach (var reading in channel.Reader.ReadAllAsync(cancellationToken))
        {
            
            var model = MapToReadingModel(reading);
            yield return model;
        }
    }

   
   
}

static ReadingModelV1 MapToReadingModel(Reading reading)
{
    return new 
        ReadingModelV1(reading.Id, reading.RawJson, reading.Meter.MeterNumber, reading.IngestionDate, reading.Meter.Group.Id, reading.Meter.Site.Id);
}

static SiteModelV1 MapToSiteModel(Site site)
{
    return new SiteModelV1(site.Id, site.Name, site.Latitude, site.Longitude, site.Altitude, site.Grid.ToString());
}

static GroupModelV1 MapToGroupModel(Group group)
{
    return new GroupModelV1(group.Id, group.Name);
}



static bool IsDesignTime()
{
    
    bool hasDesignTimeEnvVar = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_DESIGN_TIME_HOST"));
    
    bool isCommandLineTool = AppDomain.CurrentDomain.GetAssemblies()
        .Any(a => a.GetName().Name?.Contains("EntityFrameworkCore.Design") == true);
    
    bool hasDesignAssemblies = AppDomain.CurrentDomain.GetAssemblies()
        .Any(a => a.GetName().Name?.Contains("dotnet-ef") == true);
        
    return hasDesignTimeEnvVar || isCommandLineTool || hasDesignAssemblies;
}