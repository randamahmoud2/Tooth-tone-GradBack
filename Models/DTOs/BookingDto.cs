namespace DentalManagementAPI.Models.DTOs
{
    public class BookingResponseDto
    {
        public int Id { get; set; }
        public string PatientName { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Status { get; set; }
        public decimal Fee { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime BookingDate { get; set; }
        public PatientDto Patient { get; set; }
        public DoctorDto Doctor { get; set; }
    }
}