namespace DentalManagementAPI.Models
{
    public class CheckOutRequest
    {

        public int AttendanceRecordId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
