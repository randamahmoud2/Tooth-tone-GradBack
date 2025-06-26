using System.ComponentModel.DataAnnotations;

namespace DentalManagementAPI.Models
{
    /// <summary>
    /// Request model for user login
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// User's email address
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        /// <summary>
        /// User's password
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        /// <summary>
        /// User's role (e.g., doctor, patient, receptionist, manager)
        /// </summary>
        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; }
    }

    /// <summary>
    /// Request model for user signup
    /// </summary>
    public class SignupRequest
    {
        /// <summary>
        /// Full name of the user
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        /// <summary>
        /// User's email address
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        /// <summary>
        /// User's password
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        /// <summary>
        /// User's role (e.g., doctor, patient, receptionist, manager)
        /// </summary>
        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; }

        /// <summary>
        /// User's gender
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// User's age
        /// </summary>
        public int? Age { get; set; }

        /// <summary>
        /// User's national ID
        /// </summary>
        public string NationalId { get; set; }

        /// <summary>
        /// User's address
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Doctor's specialty (required for doctors)
        /// </summary>
        public string? Specialty { get; set; }

        /// <summary>
        /// User's phone number
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// User's date of birth
        /// </summary>
        public DateTime? Dob { get; set; }



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































    }
}