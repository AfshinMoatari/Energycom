using Energycom.Analysis.Models;
using Energycom.Ingestion.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Implements data access methods for readings, devices, and analytics using EF Core.
/// </summary>
public class ReadingRepository : IReadingRepository
{
    private readonly ECOMDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadingRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The EF Core database context.</param>
    public ReadingRepository(ECOMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DapperReading>> GetAllReadingsAsync(CancellationToken cancellationToken)
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

    /// <inheritdoc/>
    public async Task<IEnumerable<MeterInfo>> GetAllDevicesAsync(CancellationToken cancellationToken)
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

    /// <inheritdoc/>
    public async Task<IEnumerable<DeviceStats>> GetPerDeviceStatsAsync(CancellationToken cancellationToken)
    {
        var readings = await GetAllReadingsAsync(cancellationToken);
        return readings
            .GroupBy(r => r.MeterNumber)
            .Select(g =>
            {
                int skippedProduced, skippedConsumed;
                var produced = EnergyCalculator.CalculateProduced(g, out skippedProduced);
                var consumed = EnergyCalculator.CalculateConsumed(g, out skippedConsumed);
                return new DeviceStats
                {
                    MeterNumber = g.Key,
                    TotalProduced = produced,
                    TotalConsumed = consumed,
                    ReadingCount = g.Count(),
                    SkippedProduced = skippedProduced,
                    SkippedConsumed = skippedConsumed,
                    FirstReading = g.Min(r => r.Timestamp),
                    LastReading = g.Max(r => r.Timestamp)
                };
            })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GroupStats>> GetGroupStatsAsync(CancellationToken cancellationToken)
    {
        var readings = await GetAllReadingsAsync(cancellationToken);
        return readings
            .GroupBy(r => r.GroupName)
            .Select(g =>
            {
                int skipped;
                var produced = EnergyCalculator.CalculateProduced(g, out skipped);
                return new GroupStats
                {
                    GroupName = g.Key,
                    TotalProduced = produced,
                    DeviceCount = g.Select(r => r.MeterNumber).Distinct().Count(),
                    ReadingCount = g.Count()
                };
            })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DeviceQuality>> GetDeviceQualityAsync(CancellationToken cancellationToken)
    {
        var readings = await GetAllReadingsAsync(cancellationToken);
        return readings
            .GroupBy(r => r.MeterNumber)
            .Select(g =>
            {
                int skipped;
                EnergyCalculator.CalculateNet(g, out skipped);
                return new DeviceQuality
                {
                    MeterNumber = g.Key,
                    SkippedReadings = skipped,
                    TotalReadings = g.Count()
                };
            })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DailyProduction>> GetDailyProductionAsync(CancellationToken cancellationToken)
    {
        var readings = await GetAllReadingsAsync(cancellationToken);
        return readings
            .GroupBy(r => r.Timestamp.Date)
            .Select(g =>
            {
                int skipped;
                var produced = EnergyCalculator.CalculateProduced(g, out skipped);
                return new DailyProduction
                {
                    Date = g.Key,
                    TotalProduced = produced,
                    Skipped = skipped
                };
            })
            .OrderBy(d => d.Date)
            .ToList();
    }
}