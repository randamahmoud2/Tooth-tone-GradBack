using DentalManagementAPI.Models.DTOs;

namespace DentalManagementAPI.Models
{
    public class PatientDocument
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public PatientDto Patient { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime UploadDate { get; set; }
        public string DocumentUrl { get; set; }
        public string DocumentType { get; set; } // "Report", "Consent", "Invoice", etc.
    }
}
