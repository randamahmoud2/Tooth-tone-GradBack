using System.ComponentModel.DataAnnotations;
using DentalManagementAPI.Models;

namespace DentalManagementAPI.Models.DTOs
{
    public class AdminEarningsDto
    {
        [Key] // Add this

        public int? Id { get; set; }

        public decimal? TotalEarningsFees { get; set; } // ✅ ده متوافق مع SQL DECIMAL


    }
}