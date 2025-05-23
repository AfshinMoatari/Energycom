namespace Energycom.Analysis.Models
{
    public class DeviceStats
    {
        public string MeterNumber { get; set; }
        public double TotalProduced { get; set; }
        public double TotalConsumed { get; set; }
        public int ReadingCount { get; set; }
        public int SkippedProduced { get; set; }
        public int SkippedConsumed { get; set; }
        public DateTime FirstReading { get; set; }
        public DateTime LastReading { get; set; }
    }
}
