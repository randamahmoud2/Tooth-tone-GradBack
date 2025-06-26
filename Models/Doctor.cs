using DentalManagementAPI.Models;

public class Doctor
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Specialty { get; set; }
    public string Bio { get; set; }
    public string Location { get; set; }
    public decimal Fee { get; set; }
    public bool IsActive { get; set; }
    public string ImageUrl { get; set; }
    // العلاقة مع User
    public int UserId { get; set; }
    public User User { get; set; }
    public List<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}