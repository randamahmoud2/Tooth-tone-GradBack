namespace DentalManagementAPI.Models
{
    public class CheckInRequest
    {

        public int DoctorId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

}
