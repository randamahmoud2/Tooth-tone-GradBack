namespace DentalManagementAPI.Models
{
    public class ToothProcedure
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int ToothNumber { get; set; }
        public int ProcedureId { get; set; }
        public DateTime Date { get; set; }
        public decimal Cost { get; set; }
        // public string Status { get; set; } // Optional, uncomment if needed
    }
}