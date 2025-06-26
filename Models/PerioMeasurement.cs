namespace DentalManagementAPI.Models
{
    public class PerioMeasurement
    {
        public int Id { get; set; }
        public int PerioChartId { get; set; }
        public PerioChart PerioChart { get; set; }
        public int ToothNumber { get; set; }
        public decimal GingivalMargin { get; set; }
        public decimal PocketDepth { get; set; }
        public bool BleedingOnProbing { get; set; }
        public int PlaqueIndex { get; set; }
    }
}
