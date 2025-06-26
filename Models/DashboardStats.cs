namespace DentalManagementAPI.Models
{
    public class DashboardStats
    {
        public decimal TotalEarnings { get; set; }
        public int AppointmentsCount { get; set; }
        public int PatientsCount { get; set; }
        public List<Booking> RecentBookings { get; set; }
    }
}
