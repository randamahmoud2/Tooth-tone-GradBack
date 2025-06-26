using DentalManagementAPI.Data;
namespace DentalManagementAPI.Models
{
    public class Medication
    {
        public int Id { get; set; }
        public int PrescriptionId { get; set; }
        public Prescription? Prescription { get; set; }
        public string Drug { get; set; }
        public string Strength { get; set; }
        public int Dispense { get; set; }
        public int Refill { get; set; }
        public int Expiration { get; set; }
        public string PatientInstruct { get; set; }
    }
}
