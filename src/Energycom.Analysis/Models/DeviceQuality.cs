namespace Energycom.Analysis.Models
{
    public class DeviceQuality
    {
        public string MeterNumber { get; set; }
        public int SkippedReadings { get; set; }
        public int TotalReadings { get; set; }
    }
}
