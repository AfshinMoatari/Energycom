/// <summary>
/// Provides methods for printing energy and device analytics to the console.
/// </summary>
public interface IEnergyAnalysisService
{
    /// <summary>
    /// Prints a summary table of net, produced, and consumed electricity.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task PrintAllReadingsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Prints a table of all devices with their details.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task PrintAllDevicesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Prints statistics for each device, including production, consumption, and reading counts.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task PrintDeviceStatsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Prints statistics for each group, including total production and device count.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task PrintGroupStatsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Prints data quality metrics for each device, such as skipped and total readings.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task PrintDeviceQualityAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Prints daily production statistics, including total produced energy and skipped readings per day.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task PrintDailyProductionAsync(CancellationToken cancellationToken);
}
