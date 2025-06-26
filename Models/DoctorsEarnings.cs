using System.ComponentModel.DataAnnotations;
using DentalManagementAPI.Models;

public class DoctorsEarnings
{
    [Key] // Add this

    public int UserId { get; set; }
    public string Account { get; set; }
    public string Name { get; set; }

    public double TotalEarning { get; set; }


}