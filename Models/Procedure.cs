namespace DentalManagementAPI.Models
{
    public class Procedure
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public decimal Cost { get; set; }
    }
}