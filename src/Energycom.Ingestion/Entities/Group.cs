namespace Energycom.Ingestion.Entities;

public class Group
{
    
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public List<Meter> Meters { get; set; }
    
}