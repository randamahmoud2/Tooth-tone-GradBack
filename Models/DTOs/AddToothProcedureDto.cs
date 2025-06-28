namespace DentalManagementAPI.Models.DTOs
{
    public class AddToothProcedureDto
    {
        public int ToothNumber { get; set; }
        public int ProcedureId { get; set; }
        public decimal Cost { get; set; }
        // public string Status { get; set; } // Optional, uncomment if needed
    }
}