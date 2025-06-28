namespace DentalManagementAPI.Models.DTOs
{
    public class ToothProcedureDto
    {
        public int Id { get; set; }
        public int ToothNumber { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Cost { get; set; }
        public string Date { get; set; }
        // public string Status { get; set; } // Optional, uncomment if needed
    }
}