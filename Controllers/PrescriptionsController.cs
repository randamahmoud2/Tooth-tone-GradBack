using DentalManagementAPI.Data;
using DentalManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DentalManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PrescriptionsController> _logger;

        public PrescriptionsController(AppDbContext context, ILogger<PrescriptionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreatePrescription([FromBody] Prescription prescription)
        {
            try
            {
                if (prescription == null)
                {
                    return BadRequest("Prescription data is null");
                }

                // Validate patientId
                var patientExists = await _context.Patients.AnyAsync(p => p.Id == prescription.PatientId);
                if (!patientExists)
                {
                    return NotFound("Patient not found");
                }

                // Validate doctorId
                var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == prescription.DoctorId);
                if (!doctorExists)
                {
                    return NotFound("Doctor not found");
                }

                // Validate medications
                if (prescription.Medications == null || !prescription.Medications.Any())
                {
                    return BadRequest("At least one medication is required");
                }

                foreach (var med in prescription.Medications)
                {
                    if (string.IsNullOrEmpty(med.Drug) || string.IsNullOrEmpty(med.PatientInstruct) || med.Expiration <= 0)
                    {
                        return BadRequest("Medication must have a name, patient instructions, and valid expiration days");
                    }
                    med.PrescriptionId = prescription.Id; // Ensure foreign key is set
                }

                // Set defaults
                if (prescription.Date == default)
                {
                    prescription.Date = DateTime.UtcNow;
                }
                if (string.IsNullOrEmpty(prescription.ModifiedBy))
                {
                    prescription.ModifiedBy = "System"; // Default value
                }

                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();

                // Return DTO to avoid serialization cycle
                var response = new
                {
                    prescription.Id,
                    prescription.PatientId,
                    prescription.DoctorId,
                    prescription.Date,
                    prescription.Status,
                    prescription.AllowSubstitution,
                    prescription.PharmacyInstructions,
                    prescription.ModifiedBy,
                    Medications = prescription.Medications.Select(m => new
                    {
                        m.Id,
                        m.Drug,
                        m.Strength,
                        m.Dispense,
                        m.Refill,
                        m.Expiration,
                        m.PatientInstruct
                    }).ToList()
                };

                return CreatedAtAction(nameof(GetPrescription), new { id = prescription.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prescription");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetPrescriptionsByPatient(int patientId)
        {
            try
            {
                var prescriptions = await _context.Prescriptions
                    .Where(p => p.PatientId == patientId)
                    .Include(p => p.Medications)
                    .Include(p => p.Doctor)
                    .Select(p => new
                    {
                        p.Id,
                        p.Date,
                        DoctorName = p.Doctor != null ? p.Doctor.Name : "Not available",
                        p.ModifiedDate,
                        p.ModifiedBy,
                        p.Status,
                        p.AllowSubstitution,
                        p.PharmacyInstructions,
                        Medications = p.Medications.Select(m => new
                        {
                            m.Id,
                            m.Drug,
                            m.Strength,
                            m.Dispense,
                            m.Refill,
                            m.Expiration,
                            m.PatientInstruct
                        })
                    })
                    .ToListAsync();

                return Ok(prescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching prescriptions for patient {patientId}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetPrescription(int id)
        {
            try
            {
                var prescription = await _context.Prescriptions
                    .Include(p => p.Medications)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (prescription == null)
                {
                    return NotFound("Prescription not found");
                }

                // Return DTO to avoid serialization cycle
                var response = new
                {
                    prescription.Id,
                    prescription.PatientId,
                    prescription.DoctorId,
                    prescription.Date,
                    prescription.Status,
                    prescription.AllowSubstitution,
                    prescription.PharmacyInstructions,
                    prescription.ModifiedBy,
                    Medications = prescription.Medications.Select(m => new
                    {
                        m.Id,
                        m.Drug,
                        m.Strength,
                        m.Dispense,
                        m.Refill,
                        m.Expiration,
                        m.PatientInstruct
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching prescription with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePrescription(int id, [FromBody] Prescription prescription)
        {
            try
            {
                if (id != prescription.Id)
                {
                    return BadRequest("ID mismatch");
                }

                var existingPrescription = await _context.Prescriptions
                    .Include(p => p.Medications)
                    .FirstOrDefaultAsync(p => p.Id == id);
                if (existingPrescription == null)
                {
                    return NotFound("Prescription not found");
                }

                var patientExists = await _context.Patients.AnyAsync(p => p.Id == prescription.PatientId);
                if (!patientExists)
                {
                    return NotFound("Patient not found");
                }

                var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == prescription.DoctorId);
                if (!doctorExists)
                {
                    return NotFound("Doctor not found");
                }

                existingPrescription.PatientId = prescription.PatientId;
                existingPrescription.DoctorId = prescription.DoctorId;
                existingPrescription.Date = prescription.Date;
                existingPrescription.Status = prescription.Status;
                existingPrescription.AllowSubstitution = prescription.AllowSubstitution;
                existingPrescription.PharmacyInstructions = prescription.PharmacyInstructions;
                existingPrescription.ModifiedDate = DateTime.UtcNow;
                existingPrescription.ModifiedBy = prescription.ModifiedBy ?? "System"; // Default value

                _context.Medications.RemoveRange(existingPrescription.Medications);
                existingPrescription.Medications = prescription.Medications;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating prescription with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrescription(int id)
        {
            try
            {
                var prescription = await _context.Prescriptions.FindAsync(id);
                if (prescription == null)
                {
                    return NotFound("Prescription not found");
                }

                _context.Prescriptions.Remove(prescription);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting prescription with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}