using Energycom.Analysis.Models;
using Energycom.Ingestion.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
        public async Task PrintAllDevicesAsync(CancellationToken cancellationToken)
        {
            var devices = await GetAllDevicesAsync(cancellationToken);
            Console.WriteLine("+----+--------------+-----------+-----------+----------+----------+----------+-----------+");
            Console.WriteLine("| Id | MeterNumber  | Group     | Site      | Latitude | Longitude| Altitude | TimeZone  |");
            Console.WriteLine("+----+--------------+-----------+-----------+----------+----------+----------+-----------+");
            foreach (var d in devices)
            {
                Console.WriteLine($"| {d.Id,2} | {d.MeterNumber,-12} | {d.GroupName,-9} | {d.SiteName,-9} | {d.Latitude,8} | {d.Longitude,8} | {d.Altitude,8} | {d.TimeZone,-9} |");
            }
            Console.WriteLine("+----+--------------+-----------+-----------+----------+----------+----------+-----------+");
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
        private async Task<IEnumerable<MeterInfo>> GetAllDevicesAsync(CancellationToken cancellationToken)
        {
            var meters = await _dbContext.Meters
                .AsNoTracking()
                .Include(m => m.Group)
                .Include(m => m.Site)
                .Include(m => m.Configuration)
                .ToListAsync(cancellationToken);

            return meters.Select(m => new MeterInfo
            {
                Id = m.Id,
                MeterNumber = m.MeterNumber,
                GroupName = m.Group?.Name,
                SiteName = m.Site?.Name,
                Latitude = m.Site?.Latitude,
                Longitude = m.Site?.Longitude,
                Altitude = m.Site?.Altitude,
                TimeZone = m.Site?.TimeZone,
                Configuration = m.Configuration
            });
        }

    }
}
