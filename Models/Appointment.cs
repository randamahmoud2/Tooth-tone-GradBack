using DentalManagementAPI.Models.DTOs;

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string TimeSlot { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int DoctorId { get; set; }
    public PatientDto Patient { get; set; }
    public Doctor Doctor { get; set; }
    public string Status { get; set; } = "Pending";
}