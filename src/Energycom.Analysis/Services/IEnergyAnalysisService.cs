public interface IEnergyAnalysisService
{
    Task PrintAllReadingsAsync(CancellationToken cancellationToken);
}
