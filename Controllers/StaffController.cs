using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DentalManagementAPI.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DentalManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StaffController> _logger;

        public StaffController(AppDbContext context, ILogger<StaffController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Staff/count
        [HttpGet("count")]
        public async Task<ActionResult<object>> GetStaffCount()
        {
            try
            {
                var total = await _context.Users
                    .Where(u => (u.Role.ToLower() == "doctor" || u.Role.ToLower() == "receptionist") && (u.Status == "Active" || u.Status == null || u.Status == ""))
                    .CountAsync();
                var doctors = await _context.Users
                    .Where(u => u.Role.ToLower() == "doctor" && (u.Status == "Active" || u.Status == null || u.Status == ""))
                    .CountAsync();
                var receptionists = await _context.Users
                    .Where(u => u.Role.ToLower() == "receptionist" && (u.Status == "Active" || u.Status == null || u.Status == ""))
                    .CountAsync();
                return Ok(new { total, doctors, receptionists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching staff count");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/Staff
        [HttpGet]
        public async Task<ActionResult<object>> GetStaffList()
        {
            try
            {
                var staff = await _context.Users
                    .Where(u => u.Role.ToLower() == "doctor" || u.Role.ToLower() == "receptionist")
                    .Select(u => new
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Email = u.Email,
                        Role = u.Role,
                        Joined = u.Joined != null ? u.Joined.Value.ToString("M/d/yyyy") : "N/A",
                        Status = u.Status == "" ? "Active" : (u.Status ?? "Active"),
                        Action = u.Action == "" ? "Remove" : (u.Action ?? "Remove")
                    })
                    .ToListAsync();
                return Ok(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching staff list");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // PUT: api/Staff/{id}/deactivate
        [HttpPut("{id}/deactivate")]
        public async Task<ActionResult<object>> DeactivateStaff(int id)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == id && (u.Role.ToLower() == "doctor" || u.Role.ToLower() == "receptionist"))
                    .FirstOrDefaultAsync();
                if (user == null)
                {
                    return NotFound(new { message = "Staff member not found" });
                }
                user.Status = "Inactive";
                user.Action = "Remove";
                await _context.SaveChangesAsync();
                return Ok(new { message = "Staff member deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deactivating staff with ID {id}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}