using DentalManagementAPI.Models;
namespace DentalManagementAPI.Models.DTOs
{
    public class PatientDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public DateTime Dob { get; set; }
        public string NationalId { get; set; }
        public string Address { get; set; }


        public int UserId { get; set; }
        public User User { get; set; }

        // New nullable properties
        public string? Allergies { get; set; }
        public string? BloodType { get; set; }
        public string? ChronicDiseases { get; set; }
        public string? Governorate { get; set; }
        public string? InsuranceNumber { get; set; }
        public string? MaritalStatus { get; set; }
        public string? PregnancyStatus { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? InsuranceProvider { get; set; }
        public DateTime? InsuranceExpiry { get; set; }

        public List<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Prescription> Prescriptions { get; set; }

        public List<PatientImage> Images { get; set; } = new List<PatientImage>();
        public List<PatientDocument> Documents { get; set; } = new List<PatientDocument>();
        public List<PerioChart> PerioCharts { get; set; } = new List<PerioChart>();
    }
}