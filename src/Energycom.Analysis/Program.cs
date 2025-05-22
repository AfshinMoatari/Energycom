using System.Net.Http.Json;
using Dapper;
using Energycom.Ingestion.Data;
using Energycom.Ingestion.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddHttpClient("ingestion", conf => 
    {
        conf.BaseAddress = new Uri("https://Ingestion");
    }).AddServiceDiscovery();
builder.Services.AddHostedService<ConsoleApp>();

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


var host = builder.Build();
await host.RunAsync();


public class ConsoleApp(
    IHostApplicationLifetime lifetime, 
    IHttpClientFactory factory, 
    ECOMDbContext ecomDbContext,
    ILogger<ConsoleApp> logger)
    : IHostedService, IDisposable
{
    private readonly HttpClient _httpclient = factory.CreateClient("ingestion");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Console app started");
            
            await SamplePeriodicCall(cancellationToken);
            //await SampleStreamFunction(cancellationToken);
            //await SampleEFCoreQuery(cancellationToken);
            //await SampleDapperQuery(cancellationToken);
            
            logger.LogInformation("Work completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in application");
        }
        finally
        {
            // Stop the application when done
            lifetime.StopApplication();
        }
    }

    
       
    record DapperReading(Guid Id, string RawJson, DateTime Timestamp, int MeterId, string MeterNumber, string GroupName);

    
    /// <summary>
    /// This function uses Dapper to query the database and get readings with their associated meter and group and converts them to a flattened query object.
    /// </summary>
    /// <param name="cancellationToken"></param>
    private async Task SampleDapperQuery(CancellationToken cancellationToken)
    {
        var connection = ecomDbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);


        var sql =
            @"SELECT r.Id, r.raw_json as RawJson, r.ingestion_date as TimeStamp, r.meter_id as MeterId, m.meter_number as MeterNumber, g.Name as GroupName
                    FROM readings r
                    inner join meters m on r.meter_id = m.id
                    inner join groups g on g.id = m.group_id;";
        var readings = await connection.QueryAsync<DapperReading>(sql);

        foreach (var reading in readings)
        {
            Console.WriteLine(reading.RawJson);
        }
    }


    /// <summary>
    /// This function uses an IAsyncEnumerable to get a stream of data from the API and continuously process it.
    /// </summary>
    /// <param name="cancellationToken"></param>
    private async Task SampleStreamFunction(CancellationToken cancellationToken)
    {
        // Example of using the HttpClient to get a stream of data
        await foreach (var item in _httpclient.GetFromJsonAsAsyncEnumerable<ReadingModelV1>("/readings/stream", cancellationToken: cancellationToken))
        {
            // Process item
            Console.WriteLine(item);
        }
    }

    /// <summary>
    /// This function makes periodic calls to the API to get readings between two timestamps with an inclusive lower bound and exclusive upper bound.
    /// </summary>
    /// <param name="cancellationToken"></param>
    private async Task SamplePeriodicCall(CancellationToken cancellationToken)
    {
        var fromTime = DateTime.UtcNow.AddSeconds(-15);
        
        while(!cancellationToken.IsCancellationRequested)
        {
            var toTime = DateTime.UtcNow;
            
            var fromTimeString = fromTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var toTimeString = toTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var readings = await _httpclient.GetFromJsonAsync<List<ReadingModelV1>>($"/readings/{fromTimeString}/{toTimeString}", cancellationToken: cancellationToken);

            //move time forward for next call
            fromTime = toTime;
            if (readings is null)
            {
                logger.LogWarning("No readings found between {From} and {To}", fromTime, toTime);
                continue;
            }
            
            foreach (var reading in readings)
            {
                // Process each reading
                Console.WriteLine(reading);
            }
           
            await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
        }
    }
    
    /// <summary>
    /// This function uses EF Core to query the database and get readings with their associated meter and group.
    /// </summary>
    /// <param name="cancellationToken"></param>
    private async Task SampleEFCoreQuery(CancellationToken cancellationToken)
    {

        var readings = await ecomDbContext.Readings
            .Include(e => e.Meter)
            .ThenInclude(e => e.Group).ToListAsync(cancellationToken);

        foreach (var reading in readings)
        {
            Console.WriteLine(reading.RawJson);
        }
    }
    

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Console app stopped");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _httpclient.Dispose();
    }
}