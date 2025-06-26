using DentalManagementAPI.Models.DTOs;

namespace DentalManagementAPI.Models
{
    public class PerioChart
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public PatientDto Patient { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; }
        public List<PerioMeasurement> Measurements { get; set; }
    }
}
