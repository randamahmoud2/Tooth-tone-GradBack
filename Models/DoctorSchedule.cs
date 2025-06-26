namespace DentalManagementAPI.Models
{
    public class DoctorSchedule
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }
        // 0 = Sunday, 1 = Monday, ..., 6 = Saturday
        public int DayOfWeek { get; set; }

        // مثلا "10:00 AM", "2:00 PM", ...
        public string TimeSlot { get; set; }
        public bool IsAvailable { get; set; }
        public Doctor Doctor { get; set; }
    }
}
