using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DentalManagementAPI.Data;
using DentalManagementAPI.Models;
using DentalManagementAPI.Models.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DentalManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DentalChartController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DentalChartController> _logger;

        public DentalChartController(AppDbContext context, ILogger<DentalChartController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/DentalChart/procedures
        [HttpGet("procedures")]
        public async Task<ActionResult<object>> GetProcedures()
        {
            try
            {
                var procedures = await _context.Procedures
                    .Select(p => new ProcedureDto
                    {
                        Id = p.Id,
                        Type = p.Type,
                        Name = p.Name,
                        Cost = p.Cost.ToString("F2")
                    })
                    .ToListAsync();
                return Ok(procedures);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching procedures");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/DentalChart/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<object>> GetPatientProcedures(int patientId)
        {
            try
            {
                var procedures = await _context.ToothProcedures
                    .Where(tp => tp.PatientId == patientId)
                    .Join(
                        _context.Procedures,
                        tp => tp.ProcedureId,
                        p => p.Id,
                        (tp, p) => new ToothProcedureDto
                        {
                            Id = tp.Id,
                            ToothNumber = tp.ToothNumber,
                            Type = p.Type,
                            Name = p.Name,
                            Cost = tp.Cost.ToString("F2"),
                            Date = tp.Date.ToString("M/d/yyyy")
                            // Status = tp.Status // Uncomment if Status is enabled
                        }
                    )
                    .ToListAsync();
                return Ok(procedures);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching procedures for patient {patientId}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // POST: api/DentalChart/patient/{patientId}/procedure
        [HttpPost("patient/{patientId}/procedure")]
        public async Task<ActionResult<object>> AddPatientProcedure(int patientId, [FromBody] AddToothProcedureDto dto)
        {
            try
            {
                var procedure = new ToothProcedure
                {
                    PatientId = patientId,
                    ToothNumber = dto.ToothNumber,
                    ProcedureId = dto.ProcedureId,
                    Date = DateTime.Now,
                    Cost = dto.Cost
                    // Status = dto.Status ?? "Done" // Uncomment if Status is enabled
                };

                _context.ToothProcedures.Add(procedure);
                await _context.SaveChangesAsync();

                var result = await _context.ToothProcedures
                    .Where(tp => tp.Id == procedure.Id)
                    .Join(
                        _context.Procedures,
                        tp => tp.ProcedureId,
                        p => p.Id,
                        (tp, p) => new ToothProcedureDto
                        {
                            Id = tp.Id,
                            ToothNumber = tp.ToothNumber,
                            Type = p.Type,
                            Name = p.Name,
                            Cost = tp.Cost.ToString("F2"),
                            Date = tp.Date.ToString("M/d/yyyy")
                            // Status = tp.Status // Uncomment if Status is enabled
                        }
                    )
                    .FirstOrDefaultAsync();

                return CreatedAtAction(nameof(GetPatientProcedures), new { patientId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding procedure for patient {patientId}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // DELETE: api/DentalChart/patient/procedure/{procedureId}
        [HttpDelete("patient/procedure/{procedureId}")]
        public async Task<ActionResult<object>> DeletePatientProcedure(int procedureId)
        {
            try
            {
                var procedure = await _context.ToothProcedures.FindAsync(procedureId);
                if (procedure == null)
                {
                    return NotFound(new { message = "Procedure not found" });
                }

                _context.ToothProcedures.Remove(procedure);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Procedure deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting procedure {procedureId}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}