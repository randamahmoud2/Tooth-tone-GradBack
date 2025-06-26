using System.ComponentModel.DataAnnotations;
namespace DentalManagementAPI.Models.DTOs
{
    public class DoctorEarningDTO
    {
        [Key] // Add this

        public int? UserId { get; set; }
        public string? Account { get; set; }
        public string? Name { get; set; }
        public decimal? TotalEarning { get; set; }
    }
}