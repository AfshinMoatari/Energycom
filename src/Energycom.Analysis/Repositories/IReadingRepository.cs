using Energycom.Analysis.Models;

/// <summary>
/// Provides data access methods for readings, devices, and analytics.
/// </summary>
public interface IReadingRepository
{
    /// <summary>
    /// Retrieves all readings from the database, including related meter and group information.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A collection of <see cref="DapperReading"/> objects.</returns>
    Task<IEnumerable<DapperReading>> GetAllReadingsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all devices (meters) from the database, including related group, site, and configuration information.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A collection of <see cref="MeterInfo"/> objects.</returns>
    Task<IEnumerable<MeterInfo>> GetAllDevicesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves per-device statistics, such as total produced/consumed energy, reading counts, and skipped readings.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A collection of <see cref="DeviceStats"/> objects.</returns>
    Task<IEnumerable<DeviceStats>> GetPerDeviceStatsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves per-group statistics, such as total produced energy and device count.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A collection of <see cref="GroupStats"/> objects.</returns>
    Task<IEnumerable<GroupStats>> GetGroupStatsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves data quality metrics for each device, such as skipped and total readings.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A collection of <see cref="DeviceQuality"/> objects.</returns>
    Task<IEnumerable<DeviceQuality>> GetDeviceQualityAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves daily production statistics, including total produced energy and skipped readings per day.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A collection of <see cref="DailyProduction"/> objects.</returns>
    Task<IEnumerable<DailyProduction>> GetDailyProductionAsync(CancellationToken cancellationToken);
}