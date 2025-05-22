namespace Energycom.Ingestion.Models;

public record ReadingModelV1(Guid ReadingId, string ReadingRawJson, string MeterMeterNumber, DateTime ReadingReadingDate, int GroupId, int SiteId);

public record SiteModelV1(int Id, string Name, decimal Latitude, decimal Longitude, decimal Altitude, string AssociatedGrid);


public record GroupModelV1(int Id, string Name);