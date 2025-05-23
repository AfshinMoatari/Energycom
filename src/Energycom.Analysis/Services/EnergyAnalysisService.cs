using Energycom.Ingestion.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Energycom.Analysis.Services
{
    public class EnergyAnalysisService : IEnergyAnalysisService
    {
        private readonly ECOMDbContext _dbContext;
        private readonly ILogger<EnergyAnalysisService> _logger;

        public EnergyAnalysisService(ECOMDbContext dbContext, ILogger<EnergyAnalysisService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task PrintAllReadingsAsync(CancellationToken cancellationToken)
        {
            var readings = await GetAllReadingsAsync(cancellationToken);

            int skippedNet, skippedProduced, skippedConsumed;

            var net = EnergyCalculator.CalculateNet(readings, out skippedNet);
            var produced = EnergyCalculator.CalculateProduced(readings, out skippedProduced);
            var consumed = EnergyCalculator.CalculateConsumed(readings, out skippedConsumed);

            Console.WriteLine();
            Console.WriteLine("+-------------------+-------------------+-------------------+");
            Console.WriteLine("|      Metric       |     Value (kWh)   |     Skipped       |");
            Console.WriteLine("+-------------------+-------------------+-------------------+");
            Console.WriteLine($"| Net electricity   | {net,17:N2} | {skippedNet,17} |");
            Console.WriteLine($"| Total produced    | {produced,17:N2} | {skippedProduced,17} |");
            Console.WriteLine($"| Total consumed    | {consumed,17:N2} | {skippedConsumed,17} |");
            Console.WriteLine("+-------------------+-------------------+-------------------+");
            Console.WriteLine();
        }

        private async Task<IEnumerable<DapperReading>> GetAllReadingsAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Readings
                .AsNoTracking()
                .Include(r => r.Meter)
                    .ThenInclude(m => m.Group)
                .Select(r => new DapperReading(
                    r.Id,
                    r.RawJson,
                    r.IngestionDate,
                    r.MeterId,
                    r.Meter.MeterNumber,
                    r.Meter.Group.Name
                ))
                .ToListAsync(cancellationToken);
        }
    }
}
