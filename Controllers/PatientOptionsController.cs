using DentalManagementAPI.Data;
using DentalManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DentalManagementAPI.Controllers
{
    [Route("api/patient-options")]
    [ApiController]
    public class PatientOptionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PatientOptionsController> _logger;

        public PatientOptionsController(AppDbContext context, ILogger<PatientOptionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/patient-options/{patientId}/summary
        [HttpGet("{patientId}/summary")]
        public async Task<ActionResult<object>> GetPatientSummary(int patientId)
        {
            try
            {
                var summary = await _context.Patients
                    .Where(p => p.Id == patientId)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        PrescriptionCount = p.Prescriptions.Count,
                        ImageCount = p.Images.Count,
                        DocumentCount = p.Documents.Count,
                        PerioChartCount = p.PerioCharts.Count
                    })
                    .FirstOrDefaultAsync();

                if (summary == null)
                {
                    return NotFound(new { message = "Patient not found" });
                }

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching patient summary for patient {patientId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}