namespace DentalManagementAPI.Models
{
    public class UpdateBookingStatusRequest
    {
        public int BookingId { get; set; }
        public string Status { get; set; } // "completed" or "cancelled"
    }
}
