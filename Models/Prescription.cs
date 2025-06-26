using DentalManagementAPI.Models.DTOs;

namespace DentalManagementAPI.Models
{
    public class Prescription
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public PatientDto? Patient { get; set; }
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        public DateTime Date { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public string Status { get; set; } // "Active", "Expired"
        public bool AllowSubstitution { get; set; }
        public string PharmacyInstructions { get; set; }
        public List<Medication> Medications { get; set; } = new List<Medication>();
    }
}