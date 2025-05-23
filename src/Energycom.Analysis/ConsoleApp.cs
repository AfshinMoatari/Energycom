using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class ConsoleApp(
    IHostApplicationLifetime lifetime,
    IEnergyAnalysisService analysisService,
    ILogger<ConsoleApp> logger)
    : IHostedService, IDisposable
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Console app started");

            Console.WriteLine("\n=== ENERGY SUMMARY ===");
            await analysisService.PrintAllReadingsAsync(cancellationToken);

            Console.WriteLine("\n=== DEVICE INVENTORY (All Devices) ===");
            await analysisService.PrintAllDevicesAsync(cancellationToken);

            Console.WriteLine("\n=== PER-DEVICE STATISTICS ===");
            await analysisService.PrintDeviceStatsAsync(cancellationToken);

            Console.WriteLine("\n=== GROUP PRODUCTION SUMMARY ===");
            await analysisService.PrintGroupStatsAsync(cancellationToken);

            Console.WriteLine("\n=== DEVICE DATA QUALITY ===");
            await analysisService.PrintDeviceQualityAsync(cancellationToken);

            Console.WriteLine("\n=== DAILY PRODUCTION SUMMARY ===");
            await analysisService.PrintDailyProductionAsync(cancellationToken);

            logger.LogInformation("Work completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in application");
        }
        finally
        {
            lifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Console app stopped");
        return Task.CompletedTask;
    }

    public void Dispose() { }
}
