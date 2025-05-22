namespace Energycom.Ingestion.Entities;


public class Grid
{
    private Grid(string value) { Value = value; }

    public string Value { get; private set; }

    public static Grid DK1 => new("DK1");
    public static Grid DK2 => new("DK2");

    public override string ToString()
    {
        return Value;
    }
}
public class Site
{
    public int Id { get; set; }
    
    public required Grid Grid { get; set; }
    public required string Name { get; set; } 
    
    public required decimal Latitude { get; set; }
    
    public required decimal Longitude { get; set; }
    
    public required decimal Altitude { get; set; }
    
    public required string TimeZone { get; set; }

    public List<Meter> Meters { get; set; } = [];
}