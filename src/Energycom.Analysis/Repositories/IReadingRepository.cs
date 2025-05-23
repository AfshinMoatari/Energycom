using Energycom.Analysis.Models;

public interface IReadingRepository
{
    Task<IEnumerable<DapperReading>> GetAllReadingsAsync(CancellationToken cancellationToken);
    Task<IEnumerable<MeterInfo>> GetAllDevicesAsync(CancellationToken cancellationToken);
}