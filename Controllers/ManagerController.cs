using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DentalManagementAPI.Data;
using DentalManagementAPI.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DentalManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ManagerController> _logger;

        public ManagerController(AppDbContext context, ILogger<ManagerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Manager/pending-registrations
        [HttpGet("pending-registrations")]
        public async Task<ActionResult<object>> GetPendingRegistrations()
        {
            try
            {
                var count = await _context.Users
                    .Where(u => !u.IsApproved) // IsApproved = false
                    .CountAsync();
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending registrations");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/Manager/staff/count
        [HttpGet("staff/count")]
        public async Task<ActionResult<object>> GetStaffCount()
        {
            try
            {
                var total = await _context.Users
                    .Where(u => (u.Role.ToLower() == "doctor" || u.Role.ToLower() == "receptionist") &&
                                (u.Status == "Active" || u.Status == null || u.Status == "") &&
                                u.IsApproved)
                    .CountAsync();
                var doctors = await _context.Users
                    .Where(u => u.Role.ToLower() == "doctor" &&
                                (u.Status == "Active" || u.Status == null || u.Status == "") &&
                                u.IsApproved)
                    .CountAsync();
                var receptionists = await _context.Users
                    .Where(u => u.Role.ToLower() == "receptionist" &&
                                (u.Status == "Active" || u.Status == null || u.Status == "") &&
                                u.IsApproved)
                    .CountAsync();
                return Ok(new { total, doctors, receptionists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching staff count");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/Manager/patients/count
        [HttpGet("patients/count")]
        public async Task<ActionResult<object>> GetPatientsCount()
        {
            try
            {
                var count = await _context.Users
                    .Where(u => u.Role.ToLower() == "patient" && u.IsApproved)
                    .CountAsync();
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching patients count");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/Manager/appointments/today
        [HttpGet("appointments/today")]
        public async Task<ActionResult<object>> GetTodayAppointments()
        {
            try
            {
                var today = DateTime.Today; // 2025-06-27 00:00:00
                var count = await _context.Appointments
                    .Where(a => a.AppointmentDate.Date == today && a.Status == "Confirmed")
                    .CountAsync();
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching today's appointments");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
        // Add these to your existing ManagerController
        [HttpGet("doctors")]
        public async Task<ActionResult<IEnumerable<User>>> GetDoctors()
        {
            try
            {
                var doctors = await _context.Users
                    .Where(u => u.Role.ToLower() == "doctor" && u.IsApproved)
                    .ToListAsync();
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching doctors");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("appointments/{doctorId}")]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetDoctorAppointments(int doctorId)
        {
            try
            {
                var appointments = await _context.Appointments
                    .Where(a => a.DoctorId == doctorId)
                    .Include(a => a.Patient)
                    //.Include(a => a.Procedure)
                    .ToListAsync();
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching doctor appointments");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("attendance/{date}")]
        public async Task<ActionResult<IEnumerable<AttendanceRecord>>> GetStaffAttendance(DateTime date)
        {
            try
            {
                var attendance = await _context.AttendanceRecords
                    .Where(a => a.Date.Date == date.Date)
                    //.Include(a => a.Staff)
                    .ToListAsync();
                return Ok(attendance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching staff attendance");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }





















        // GET: api/Manager/recent-registrations
        [HttpGet("recent-registrations")]
        public async Task<ActionResult<object>> GetRecentRegistrations()
        {
            try
            {
                var recentRegistrations = await _context.Users
                    .Where(u => u.Role.ToLower() != "manager" && u.Role.ToLower() != "account_manager")
                    .OrderByDescending(u => u.Joined)
                    .Take(5)
                    .Select(u => new
                    {
                        name = u.Name,
                        role = u.Role,
                        email = u.Email,
                        joined = u.Joined != null ? u.Joined.Value.ToString("M/d/yyyy") : "N/A"
                    })
                    .ToListAsync();
                return Ok(recentRegistrations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recent registrations");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}