using Energycom.Ingestion.Data;
using Microsoft.Extensions.Logging;
using Energycom.Analysis.Helpers;

namespace Energycom.Analysis.Services
{
    public class EnergyAnalysisService : IEnergyAnalysisService
    {
        private readonly ECOMDbContext _dbContext;
        private readonly ILogger<EnergyAnalysisService> _logger;
        private readonly IReadingRepository _repository;

        public EnergyAnalysisService(ECOMDbContext dbContext, ILogger<EnergyAnalysisService> logger, IReadingRepository repository)
        {
            _dbContext = dbContext;
            _logger = logger;
            _repository = repository;
        }

        public async Task PrintAllReadingsAsync(CancellationToken cancellationToken)
        {
            var readings = await _repository.GetAllReadingsAsync(cancellationToken);

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
            var devices = await _repository.GetAllDevicesAsync(cancellationToken);

            Console.WriteLine("+----+--------------+-----------+-----------+----------+----------+----------+-----------+");
            Console.WriteLine("| Id | MeterNumber  | Group     | Site      | Latitude | Longitude| Altitude | TimeZone  |");
            Console.WriteLine("+----+--------------+-----------+-----------+----------+----------+----------+-----------+");
            foreach (var d in devices)
            {
                Console.WriteLine($"| {d.Id,2} | {d.MeterNumber,-12} | {d.GroupName,-9} | {d.SiteName,-9} | {d.Latitude,8} | {d.Longitude,8} | {d.Altitude,8} | {d.TimeZone,-9} |");
            }
            Console.WriteLine("+----+--------------+-----------+-----------+----------+----------+----------+-----------+");
        }
        public async Task PrintDeviceStatsAsync(CancellationToken cancellationToken)
        {
            var stats = (await _repository.GetPerDeviceStatsAsync(cancellationToken)).OrderByDescending(s => s.TotalProduced).Take(10).ToList();

            var tableLines = new List<string>
            {
                "+--------------+----------+----------+-------+--------+--------+---------------------+---------------------+",
                "| MeterNumber  | Produced | Consumed | Count | SkProd | SkCons | FirstReading        | LastReading         |",
                "+--------------+----------+----------+-------+--------+--------+---------------------+---------------------+"
            };
            foreach (var d in stats)
            {
                tableLines.Add($"| {d.MeterNumber,-12} | {d.TotalProduced,8:N2} | {d.TotalConsumed,8:N2} | {d.ReadingCount,5} | {d.SkippedProduced,6} | {d.SkippedConsumed,6} | {d.FirstReading:yyyy-MM-dd HH:mm:ss} | {d.LastReading:yyyy-MM-dd HH:mm:ss} |");
            }
            tableLines.Add("+--------------+----------+----------+-------+--------+--------+---------------------+---------------------+");

            double max = stats.Max(s => s.TotalProduced);
            string[] colorCodes = { "31", "32", "33", "34", "35", "36" };
            int colorCount = colorCodes.Length;
            int i = 0;
            var chartLines = new List<string> { "Top 10 Devices by Production:" };
            foreach (var d in stats)
            {
                int barLength = (int)(d.TotalProduced / max * 30);
                string colorBar = ConsoleHelpers.AnsiColor(new string('#', barLength), colorCodes[i % colorCount]);
                chartLines.Add($"{d.MeterNumber,-12} | {colorBar} {d.TotalProduced:N0}");
                i++;
            }

            ConsoleHelpers.PrintSideBySide(tableLines, chartLines);
        }
        public async Task PrintDeviceQualityAsync(CancellationToken cancellationToken)
        {
            var qualities = await _repository.GetDeviceQualityAsync(cancellationToken);
            Console.WriteLine("+--------------+----------------+--------------+");
            Console.WriteLine("| MeterNumber  | SkippedReadings| TotalReadings|");
            Console.WriteLine("+--------------+----------------+--------------+");
            foreach (var q in qualities)
            {
                Console.WriteLine($"| {q.MeterNumber,-12} | {q.SkippedReadings,14} | {q.TotalReadings,12} |");
            }
            Console.WriteLine("+--------------+----------------+--------------+");
        }
        public async Task PrintDailyProductionAsync(CancellationToken cancellationToken)
        {
            var daily = (await _repository.GetDailyProductionAsync(cancellationToken)).ToList();

            var tableLines = new List<string>
            {
                "+------------+---------------+---------+",
                "| Date       | Produced (kWh)| Skipped |",
                "+------------+---------------+---------+"
            };
            foreach (var d in daily)
            {
                tableLines.Add($"| {d.Date:yyyy-MM-dd} | {d.TotalProduced,13:N2} | {d.Skipped,7} |");
            }
            tableLines.Add("+------------+---------------+---------+");

            double max = daily.Max(d => d.TotalProduced);
            string[] colorCodes = { "31", "32", "33", "34", "35", "36" };
            int colorCount = colorCodes.Length;
            int i = 0;
            var chartLines = new List<string> { "Production (kWh) over time:" };
            foreach (var d in daily)
            {
                int barLength = max > 0 ? (int)(d.TotalProduced / max * 30) : 0;
                string colorBar = ConsoleHelpers.AnsiColor(new string('#', barLength), colorCodes[i % colorCount]);
                chartLines.Add($"{d.Date:yyyy-MM-dd} | {colorBar} {d.TotalProduced:N0}");
                i++;
            }

            ConsoleHelpers.PrintSideBySide(tableLines, chartLines);
        }
        public async Task PrintGroupStatsAsync(CancellationToken cancellationToken)
        {
            var groupStats = (await _repository.GetGroupStatsAsync(cancellationToken)).ToList();

            var tableLines = new List<string>
            {
                "+-----------+---------------+-------------+",
                "| GroupName | Produced (kWh)| Device Count|",
                "+-----------+---------------+-------------+"
            };
            foreach (var g in groupStats)
            {
                tableLines.Add($"| {g.GroupName,-9} | {g.TotalProduced,13:N2} | {g.DeviceCount,11} |");
            }
            tableLines.Add("+-----------+---------------+-------------+");

            double max = groupStats.Max(gs => gs.TotalProduced);
            string[] colorCodes = { "31", "32", "33", "34", "35", "36" };
            int colorCount = colorCodes.Length;
            int i = 0;
            var chartLines = new List<string> { "Group Production (kWh):" };
            foreach (var g in groupStats)
            {
                int barLength = max > 0 ? (int)(g.TotalProduced / max * 30) : 0;
                string colorBar = ConsoleHelpers.AnsiColor(new string('#', barLength), colorCodes[i % colorCount]);
                chartLines.Add($"{g.GroupName,-9} | {colorBar} {g.TotalProduced:N0}");
                i++;
            }

            ConsoleHelpers.PrintSideBySide(tableLines, chartLines);
        }

    }
}
