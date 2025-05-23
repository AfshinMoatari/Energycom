using Energycom.Analysis.Models;
using Energycom.Ingestion.Data;
using Microsoft.EntityFrameworkCore;

public class ReadingRepository : IReadingRepository
{
    private readonly ECOMDbContext _dbContext;

    public ReadingRepository(ECOMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

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
}