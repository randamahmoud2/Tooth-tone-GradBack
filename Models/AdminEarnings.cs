using System.ComponentModel.DataAnnotations;
using DentalManagementAPI.Models;

public class AdminEarnings
{
    [Key] // Add this

    public int Id { get; set; }

    public decimal? TotalEarningsFees { get; set; } // ✅ ده متوافق مع SQL DECIMAL


}