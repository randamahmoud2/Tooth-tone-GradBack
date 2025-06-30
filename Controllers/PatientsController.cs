using DentalManagementAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using DentalManagementAPI.Models.DTOs;

namespace DentalManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PatientsController> _logger;

        public PatientsController(AppDbContext context, ILogger<PatientsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Patients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetPatients()
        {
            try
            {
                var patients = await _context.Patients
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Age,
                        p.Gender,
                        p.NationalId,
                        p.Address
                    })
                    .ToListAsync();
                return Ok(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all patients");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/Patients/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetPatient(int id)
        {
            try
            {
                var patient = await _context.Patients
                    .Where(p => p.Id == id)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Age,
                        p.Gender,
                        p.PhoneNumber,
                        p.Email,
                        p.Address,
                        p.NationalId
                    })
                    .FirstOrDefaultAsync();

                if (patient == null)
                {
                    return NotFound(new { message = "Patient not found" });
                }

                return Ok(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching patient with ID {id}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/Patients/{id}/dashboard
        [HttpGet("{id}/dashboard")]
        public async Task<ActionResult<object>> GetPatientDashboard(int id)
        {
            try
            {
                var patient = await _context.Patients
                    .Where(p => p.Id == id)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Email,
                        p.PhoneNumber,
                        p.Dob,
                        PatientId = $"P{p.Id.ToString("D5")}",
                        LastVisit = p.Bookings
                            .Where(b => b.Status == "Completed" && b.BookingDate > new DateTime(2000, 1, 1))
                            .OrderByDescending(b => b.BookingDate)
                            .Select(b => (DateTime?)b.BookingDate)
                            .FirstOrDefault(),
                        BloodType = p.BloodType ?? "N/A",
                        Status = p.Bookings.Any(b => b.Status == "Active" || b.Status == "Pending") ? "Active" : "Inactive",
                        NextAppointment = _context.Appointments
                            .Where(a => a.PatientId == p.Id && (a.Status == "Active" || a.Status == "Pending") && a.AppointmentDate >= DateTime.Today)
                            .OrderBy(a => a.AppointmentDate)
                            .Select(a => (DateTime?)a.AppointmentDate)
                            .FirstOrDefault()
                    })
                    .FirstOrDefaultAsync();

                if (patient == null)
                {
                    return NotFound(new { message = "Patient not found" });
                }

                var upcomingAppointments = await _context.Appointments
                    .Where(a => a.PatientId == id && (a.Status == "Active" || a.Status == "Pending") && a.AppointmentDate >= DateTime.Today)
                    .Include(a => a.Doctor)
                    .OrderBy(a => a.AppointmentDate)
                    .Select(a => new
                    {
                        a.Id,
                        DoctorName = a.Doctor != null ? a.Doctor.Name : "Unknown Doctor",
                        Specialty = a.Doctor != null ? a.Doctor.Specialty : "N/A",
                        Time = a.TimeSlot,
                        Date = a.AppointmentDate,
                        Status = a.Status,
                        Price = a.Doctor != null ? a.Doctor.Fee.ToString("C") : "N/A",
                        Procedure = a.Status == "Completed" ? "Completed dental visit" : "Scheduled dental appointment"
                    })
                    .ToListAsync();

                var appointmentActivities = await _context.Appointments
                    .Where(a => a.PatientId == id && a.AppointmentDate > new DateTime(2000, 1, 1))
                    .OrderByDescending(a => a.AppointmentDate)
                    .Take(5)
                    .Select(a => new
                    {
                        a.Id,
                        Type = "Appointment",
                        Title = a.Status == "Completed" ? "Dental Appointment" : "Scheduled Appointment",
                        Status = a.Status,
                        Date = a.AppointmentDate,
                        Description = a.Status == "Completed" ? "Completed dental visit" : "Scheduled dental appointment"
                    })
                    .ToListAsync();

                var paymentActivities = await _context.Payments
                    .Include(p => p.Booking)
                    .Where(p => p.Booking != null && p.Booking.PatientId == id)
                    .OrderByDescending(p => p.PaymentDate)
                    .Take(5)
                    .Select(p => new
                    {
                        p.Id,
                        Type = "Payment",
                        Title = "Payment Processed",
                        Status = p.Status,
                        Date = p.PaymentDate,
                        Description = $"Payment of {p.Amount:C} for dental services"
                    })
                    .ToListAsync();

                var recentActivities = appointmentActivities
                    .Union(paymentActivities)
                    .OrderByDescending(a => a.Date)
                    .Take(5)
                    .ToList();

                return Ok(new
                {
                    Patient = patient,
                    UpcomingAppointments = upcomingAppointments,
                    RecentActivities = recentActivities
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching dashboard for patient {id}: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // GET: api/Patients/{id}/periocharts
        [HttpGet("{id}/periocharts")]
        public async Task<ActionResult<IEnumerable<object>>> GetPatientPerioCharts(int id)
        {
            try
            {
                var perioCharts = await _context.PerioCharts
                    .Where(p => p.PatientId == id)
                    .OrderByDescending(p => p.Date)
                    .Select(p => new
                    {
                        p.Id,
                        p.Date,
                        p.Notes,
                        Measurements = p.Measurements.Select(m => new
                        {
                            m.ToothNumber,
                            m.GingivalMargin,
                            m.PocketDepth,
                            m.BleedingOnProbing,
                            m.PlaqueIndex
                        })
                    })
                    .ToListAsync();

                return Ok(perioCharts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching periodontal charts for patient {id}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/Patients/{id}/images
        [HttpGet("{id}/images")]
        public async Task<ActionResult<IEnumerable<object>>> GetPatientImages(int id)
        {
            try
            {
                var images = await _context.PatientImages
                    .Where(i => i.PatientId == id)
                    .OrderByDescending(i => i.DateTaken)
                    .Select(i => new
                    {
                        i.Id,
                        i.Title,
                        i.Description,
                        i.DateTaken,
                        i.ImageUrl,
                        i.ImageType
                    })
                    .ToListAsync();

                return Ok(images);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching images for patient {id}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/Patients/{id}/documents
        [HttpGet("{id}/documents")]
        public async Task<ActionResult<IEnumerable<object>>> GetPatientDocuments(int id)
        {
            try
            {
                var documents = await _context.PatientDocuments
                    .Where(d => d.PatientId == id)
                    .OrderByDescending(d => d.UploadDate)
                    .Select(d => new
                    {
                        d.Id,
                        d.Title,
                        d.Description,
                        d.UploadDate,
                        d.DocumentUrl,
                        d.DocumentType
                    })
                    .ToListAsync();

                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching documents for patient {id}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/Patients/{id}/name
        [HttpGet("{id}/name")]
        public async Task<ActionResult<object>> GetPatientName(int id)
        {
            try
            {
                var patient = await _context.Patients
                    .Where(p => p.Id == id)
                    .Select(p => new { p.Name })
                    .FirstOrDefaultAsync();

                if (patient == null)
                {
                    return NotFound(new { message = "Patient not found" });
                }

                return Ok(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching patient name for ID {id}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}