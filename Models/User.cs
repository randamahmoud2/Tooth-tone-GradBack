using DentalManagementAPI.Models.DTOs;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
    public string Gender { get; set; }
    public int? Age { get; set; }
    public string NationalId { get; set; }
    public string Address { get; set; }
    public bool IsApproved { get; set; }

    public Doctor Doctor { get; set; }
    public PatientDto Patient { get; set; }

    public DateTime? Joined { get; set; }
    public string Action { get; set; }
    public string Status { get; set; }
}