using Energycom.Analysis.Models;

public interface IReadingRepository
{
    Task<IEnumerable<DapperReading>> GetAllReadingsAsync(CancellationToken cancellationToken);
    Task<IEnumerable<MeterInfo>> GetAllDevicesAsync(CancellationToken cancellationToken);
    Task<IEnumerable<DeviceStats>> GetPerDeviceStatsAsync(CancellationToken cancellationToken);
    Task<IEnumerable<GroupStats>> GetGroupStatsAsync(CancellationToken cancellationToken);
    Task<IEnumerable<DeviceQuality>> GetDeviceQualityAsync(CancellationToken cancellationToken);
    Task<IEnumerable<DailyProduction>> GetDailyProductionAsync(CancellationToken cancellationToken);
}