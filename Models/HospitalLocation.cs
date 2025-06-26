namespace DentalManagementAPI.Models
{
    public class HospitalLocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int RadiusMeters { get; set; } = 200;
    }
}
