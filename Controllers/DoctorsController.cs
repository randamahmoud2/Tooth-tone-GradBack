using Microsoft.AspNetCore.Mvc;
using DentalManagementAPI.Data;
using DentalManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DentalManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DoctorsController> _logger;

        public DoctorsController(AppDbContext context, ILogger<DoctorsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Doctors
        [HttpGet]
        public async Task<IActionResult> GetDoctors()
        {
            try
            {
                var doctors = await _context.Doctors
                    .Include(d => d.User)
                    .Select(d => new
                    {
                        d.Id,
                        d.Name,
                        d.Specialty,
                      //  d.ImageUrl,
                        d.IsActive, // غيّرنا من IsAvailable إلى IsActive
                        d.Bio, // غيّرنا من About إلى Bio
                        d.Location, // غيّرنا من Address إلى Location
                        d.Fee,
                        User = d.User != null ? new { d.User.Id, d.User.Name, d.User.Email } : null
                    })
                    .ToListAsync();
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching doctors");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET: api/Doctors/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctor(int id)
        {
            try
            {
                var doctor = await _context.Doctors
                    .Include(d => d.User)
                    .Select(d => new
                    {
                        d.Id,
                        d.Name,
                        d.Specialty,
                        //d.ImageUrl,
                        d.IsActive, // غيّرنا من IsAvailable إلى IsActive
                        d.Bio, // غيّرنا من About إلى Bio
                        d.Location, // غيّرنا من Address إلى Location
                        d.Fee,
                        User = d.User != null ? new { d.User.Id, d.User.Name, d.User.Email } : null
                    })
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (doctor == null)
                {
                    return NotFound(new { message = "Doctor not found" });
                }

                return Ok(doctor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching doctor with id {id}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST: api/Doctors
        [HttpPost]
        public async Task<IActionResult> CreateDoctor([FromBody] Doctor doctor)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                doctor.IsActive = false; // Default to inactive until approved
                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDoctor), new { id = doctor.Id }, doctor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating doctor");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // PUT: api/Doctors/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] Doctor doctor)
        {
            try
            {
                if (id != doctor.Id)
                {
                    return BadRequest(new { message = "Doctor ID mismatch" });
                }

                var existingDoctor = await _context.Doctors.FindAsync(id);
                if (existingDoctor == null)
                {
                    return NotFound(new { message = "Doctor not found" });
                }

                existingDoctor.Name = doctor.Name;
                existingDoctor.Specialty = doctor.Specialty;
               // existingDoctor.ImageUrl = doctor.ImageUrl;
                existingDoctor.IsActive = doctor.IsActive; // غيّرنا من IsAvailable إلى IsActive
                existingDoctor.Bio = doctor.Bio; // غيّرنا من About إلى Bio
                existingDoctor.Location = doctor.Location; // غيّرنا من Address إلى Location
                existingDoctor.Fee = doctor.Fee;

                _context.Entry(existingDoctor).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating doctor with id {id}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // DELETE: api/Doctors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            try
            {
                var doctor = await _context.Doctors.FindAsync(id);
                if (doctor == null)
                {
                    return NotFound(new { message = "Doctor not found" });
                }

                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting doctor with id {id}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}