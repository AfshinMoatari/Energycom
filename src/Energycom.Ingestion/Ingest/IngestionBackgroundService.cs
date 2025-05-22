using Energycom.Ingestion.Data;
using Energycom.Ingestion.Entities;
using Microsoft.EntityFrameworkCore;

namespace Energycom.Ingestion.Ingest;

public class TimedHostedService : IHostedService, IDisposable
{
    
    
    
    private readonly PeriodicTimer _timer;
    private readonly ILogger<TimedHostedService> _logger;
    private readonly NotificationHub _hub;
    private readonly ECOMDbContext _context;
    private List<Meter> _meters = [];
    
    public TimedHostedService(IServiceScopeFactory serviceScopeFactory, ILogger<TimedHostedService> logger, NotificationHub hub)
    {
        _logger = logger;
        _hub = hub;
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        _context = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ECOMDbContext>();
    }

    
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ingestion Hosted Service running.");
        
        _meters = await _context.Meters
            .Include(meter => meter.Group)
            .Include(meter => meter.Configuration)
            .ToListAsync(cancellationToken: cancellationToken);


        // Start the background task
        _ = Task.Run(async () => await RunAsync(cancellationToken), cancellationToken);
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(cancellationToken))
            {
                await DoWork(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Service is stopping, which is expected
            _logger.LogInformation("Timed Hosted Service is stopping.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in timed hosted service background task");
        }
    }

    
    
    private record RawReading(decimal Value, string Unit, DateTime Timestamp);

    private async Task DoWork(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ingesting Data for {Count} Meters", _meters.Count);
        
        foreach (var meter in _meters)
        {
            _logger.LogInformation("Processing Meter: {Meter}", meter.MeterNumber);
            
            //range from 0.5 to 1.49
            var valueScalar = Random.Shared.NextDouble() + 0.5;
            // Simulate reading data from a meter
          
            
            var applyEffect = Random.Shared.NextDouble() < 0.1337;
            
            
            if (meter.Configuration.CanSkipReadings && applyEffect)
            {
              _logger.LogError("Failed to ingest reading for {MeterNumber} so we have a skipped reading. Please handle :)", meter.MeterNumber);   
            }
            
            Reading reading;
            var value = meter.Configuration.BaseValue * (decimal)valueScalar;
            if (meter.Configuration.CanHaveUnparsedReadings && applyEffect)
            {
                reading = new Reading
                {
                    Id = Guid.CreateVersion7(),
                    RawJson =  @$"{{
                        ""Value"": {Convert.ToHexString(decimal.GetBits(value)
                            .SelectMany(BitConverter.GetBytes)
                            .ToArray())},
                        ""Unit"": ""kwh"",
                        ""Timestamp"": ""{DateTime.UtcNow:O}""
                        }}",
                    MeterId = meter.Id,
                    IngestionDate = DateTime.UtcNow,
                    Meter = meter,
                };
                _logger.LogError("Ingested an erroring reading for {MeterNumber}. Please handle :)", meter.MeterNumber);
            }
            else
            {
                
                var rawReading = new RawReading(
                    Value: value,
                    Unit: "kwh",
                    Timestamp: DateTime.UtcNow
                );
                
                 reading = new Reading
                {
                    Id = Guid.CreateVersion7(),
                    RawJson =  System.Text.Json.JsonSerializer.Serialize(rawReading) ,
                    MeterId = meter.Id,
                    Meter = meter,
                    IngestionDate = DateTime.UtcNow
                };
                
            }


          
            
            _context.Add(reading);

             await _hub.Notify(reading);

            if (!meter.Configuration.CanDuplicateReadings || !applyEffect) continue;
            var duplicateReading = new Reading
            {
                Id = Guid.CreateVersion7(),
                RawJson =  reading.RawJson ,
                IngestionDate = reading.IngestionDate,
                MeterId = meter.Id,
                Meter = meter,
            };
            
            _context.Add(duplicateReading);
            
            _logger.LogError("Ingested a duplicated reading for {MeterNumber}. Please handle :)", meter.MeterNumber);
                
            await _hub.Notify(duplicateReading);
        }

        await _context.SaveChangesAsync(cancellationToken);


    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _context.Dispose();
    }
}