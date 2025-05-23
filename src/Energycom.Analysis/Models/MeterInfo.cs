using Energycom.Ingestion.Entities;

namespace Energycom.Analysis.Models
{
    public class MeterInfo
    {
        public int Id { get; set; }
        public string MeterNumber { get; set; }
        public string GroupName { get; set; }
        public string SiteName { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Altitude { get; set; }
        public string TimeZone { get; set; }
        public MeterConfiguration Configuration { get; set; }
    }
}
