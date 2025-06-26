namespace DentalManagementAPI.Models
{
    public class Drug
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Strength { get; set; }
        public int? Dispense { get; set; }
        public int? Refill { get; set; }
        public int? Expiration { get; set; }
        public string PatientInstruct { get; set; }

    }
}
