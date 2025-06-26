using System.ComponentModel.DataAnnotations;

namespace DentalManagementAPI.Models.DTOs
{
    public class AppointmentDTO
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public string TimeSlot { get; set; }

        // Optional properties (not required for creation)
        public int? Id { get; set; } = null;
        public DateTime? CreatedAt { get; set; } = null;
        public PatientDto? Patient { get; set; } = null;
        public Doctor? Doctor { get; set; } = null;
        public string? Status { get; set; } = null;
    }
}


//namespace DentalManagementAPI.Models.DTOs
//{
//    public class AppointmentDTO
//    {
//        public int Id { get; set; }
//        public int PatientId { get; set; }
//        public DateTime AppointmentDate { get; set; }
//        public string TimeSlot { get; set; }
//        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

//        public int DoctorId { get; set; }
//        public PatientDto Patient { get; set; }
//        public Doctor Doctor { get; set; }
//        public string Status { get; set; } = "Pending";

//    }
//}