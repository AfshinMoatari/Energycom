public interface IEnergyAnalysisService
{
    Task PrintAllReadingsAsync(CancellationToken cancellationToken);
    Task PrintAllDevicesAsync(CancellationToken cancellationToken);
    Task PrintDeviceStatsAsync(CancellationToken cancellationToken);
    Task PrintGroupStatsAsync(CancellationToken cancellationToken);
    Task PrintDeviceQualityAsync(CancellationToken cancellationToken);
    Task PrintDailyProductionAsync(CancellationToken cancellationToken);
}
