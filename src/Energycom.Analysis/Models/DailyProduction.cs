namespace Energycom.Analysis.Models
{
    public class DailyProduction
    {
        public DateTime Date { get; set; }
        public double TotalProduced { get; set; }
        public int Skipped { get; set; }
    }
}
