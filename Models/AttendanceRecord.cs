namespace DentalManagementAPI.Models
{
    public class AttendanceRecord
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Changed from DoctorId to UserId
        public string UserType { get; set; } // "Doctor" or "Receptionist"
        public DateTime Date { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } // "Present", "Late", "Absent", etc.
        public decimal WorkingHours { get; set; }
        public string LocationCoordinates { get; set; }
        public bool IsVerified { get; set; }
    }
}