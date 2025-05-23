public interface IEnergyAnalysisService
{
    Task PrintAllReadingsAsync(CancellationToken cancellationToken);
    Task PrintAllDevicesAsync(CancellationToken cancellationToken);
}
