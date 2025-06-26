// In Models/DTOs/AppointmentResponseDto.cs
namespace DentalManagementAPI.Models.DTOs
{
    public class AppointmentResponseDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }

        // Patient information
        public PatientDto Patient { get; set; }

        // Doctor information
        public DoctorDto Doctor { get; set; }

        // Booking information (if you want to include it)
        public BookingResponseDto Booking { get; set; }
    }
}