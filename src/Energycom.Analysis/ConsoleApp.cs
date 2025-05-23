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
