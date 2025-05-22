namespace Energycom.Ingestion.Entities;

public class Meter
{
    public required int Id { get; set; }
    
    public required int GroupId { get; set; }
    
    public Group Group { get; set; }
    
    public Site Site { get; set; }
    
    public required string MeterNumber { get; set; }
    
    public List<Reading> Readings { get; set; }
    
    public MeterConfiguration Configuration { get; set; }
    
}