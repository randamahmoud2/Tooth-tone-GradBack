using Microsoft.AspNetCore.Mvc;
using DentalManagementAPI.Models;
using DentalManagementAPI.Data;
using Microsoft.EntityFrameworkCore;
using DentalManagementAPI.Models.DTOs;
using System.Data;

namespace DentalManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate a user
        /// </summary>
        /// <param name="request">Login request details</param>
        /// <returns>User details if authenticated</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {

      
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _context.Users
                    .Include(u => u.Doctor)
                    .Include(u => u.Patient)
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null || user.Password != request.Password)
                {

                    return Unauthorized(new { message = "Invalid credentials" });
                }

                var response = new
                {
                    userId = user.Id,
                    role = user.Role,
                    doctorId = user.Doctor != null ? user.Doctor.Id : 0,
                    patientId = user.Patient != null ? user.Patient.Id : 0,
                    approved = user.IsApproved,
                    name = user.Name
                };
    
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error ");
                return StatusCode(500, new
                {
                    message = "Internal server error",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="request">Signup request details</param>
        /// <returns>Success message and user ID</returns>
        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                {
                    return BadRequest(new { message = "Email already exists" });
                }

                var user = new User
                {
                    Name = request.Name,
                    Email = request.Email,
                    Password = request.Password, 
                    Role = request.Role,
                    Gender = request.Gender,
                    Age = request.Age,
                    NationalId = request.NationalId,
                    Address = request.Address,
                    IsApproved = request.Role == "patient" || request.Role == "manager",
                    Action = "" ,
                    Joined = DateTime.Now,
                    Status = ""












                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync(); 

                if (request.Role == "doctor")
                {
                    var doctor = new Doctor
                    {
                        Name = request.Name,
                        Specialty = request.Specialty,
                        UserId = user.Id,
                        Bio = $"Dr. {request.Name} specializes in {request.Specialty}.",
                        Location = request.Address,
                        Fee = 50.00m,
                        IsActive = false,
                        ImageUrl = "C:\\Users\\alpha\\OneDrive\\Desktop\\SalmaTeam\\DentalManagementAPI_Back\\Uploads\\Images\\default-doctor.png" // Add default image URL
                    };
                    _context.Doctors.Add(doctor);
                }
                else if (request.Role == "patient")
                {
                    var patient = new PatientDto
                    {
                        Name = request.Name,
                        Email = request.Email,
                        UserId = user.Id,
                        PhoneNumber = request.PhoneNumber ?? "N/A",
                        Gender = request.Gender,
                        Age = request.Age ?? 0,
                        Dob = request.Dob ?? DateTime.Now.AddYears(-(request.Age ?? 30)),
                        NationalId = request.NationalId,
                        Address = request.Address,
                        Allergies = request.Allergies,
                        BloodType = request.BloodType,
                        ChronicDiseases = request.ChronicDiseases,
                        Governorate = request.Governorate,
                        InsuranceNumber = request.InsuranceNumber,
                        MaritalStatus = request.MaritalStatus,
                        PregnancyStatus = request.Gender == "female" ? request.PregnancyStatus : "Not applicable",
                        ProfileImageUrl = request.ProfileImageUrl ?? "default-patient.png",
                        InsuranceProvider = request.InsuranceProvider,
                        InsuranceExpiry = request.InsuranceExpiry
                    };


                    _context.Patients.Add(patient);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"User password: {request.Password}");
                _logger.LogInformation(user.Email);
                return Ok(new
                {
                    message = request.Role == "doctor" || request.Role == "receptionist"
                        ? "Registration submitted. Waiting for approval."
                        : "Account created successfully",
                    userId = user.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Signup error");
                return StatusCode(500, new
                {
                    message = "Internal server error",
                    error = ex.Message
                });
            }
        }
    
    
    
    
    }
}




////// we work in this file 