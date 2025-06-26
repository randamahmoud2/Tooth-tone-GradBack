using DentalManagementAPI.Models.DTOs;

namespace DentalManagementAPI.Models
{
    public class PatientImage
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public PatientDto Patient { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateTaken { get; set; }
        public string ImageUrl { get; set; }
        public string ImageType { get; set; } // "X-Ray", "Photo", etc.
    }
}
