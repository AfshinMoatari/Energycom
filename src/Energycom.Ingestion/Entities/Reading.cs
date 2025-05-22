namespace Energycom.Ingestion.Entities;

public class Reading
{
    public required Guid Id { get; set; }
    
    public required string RawJson { get; set; }
    
    public required  int MeterId { get; set; }
    
    public required Meter Meter { get; set; }
    
    public required DateTime IngestionDate { get; set; }
}