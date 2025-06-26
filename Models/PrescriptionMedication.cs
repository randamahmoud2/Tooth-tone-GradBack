namespace DentalManagementAPI.Models
{
    public class PrescriptionMedication
    {
        public int Id { get; set; }
        public int PrescriptionId { get; set; }
        public Prescription Prescription { get; set; }
        public string DrugName { get; set; }
        public string Strength { get; set; }
        public int Dispense { get; set; }
        public int Refill { get; set; }
        public int ExpirationDays { get; set; }
        public string PatientInstructions { get; set; }
        public string Dosage { get; set; }
    }
}
