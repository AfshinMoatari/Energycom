namespace Energycom.Ingestion.Entities;


public enum MeterType
{
    Consumption,
    Production
}
public class MeterConfiguration
{
    public required int Id { get; set; }
    
    public required MeterType Type { get; set; }
    
    public required decimal BaseValue { get; set; }
    
    public required bool CanSkipReadings { get; set; }
    
    public required bool CanDuplicateReadings { get; set; }
    
    public required bool CanHaveUnparsedReadings { get; set; }
}