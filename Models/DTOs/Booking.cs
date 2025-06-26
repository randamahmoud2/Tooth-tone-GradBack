namespace DentalManagementAPI.Models.DTOs
{
    public class BookingDto
    {

        public int Id { get; set; }
        public string PatientName { get; set; }
        public int PatientId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public bool IsPaid { get; set; }
        public decimal PaymentAmount { get; set; }
        public string Status { get; set; } // "completed", "cancelled", or null
        public int DoctorId { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal Fee { get; set; }
        public PatientDto Patient { get; set; }
        public Doctor Doctor { get; set; }
    }
    }

