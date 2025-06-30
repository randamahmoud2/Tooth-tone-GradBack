using DentalManagementAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using DentalManagementAPI.Models;

namespace DentalManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PerioChartsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<PerioChartsController> _logger;

        public PerioChartsController(AppDbContext context, IWebHostEnvironment env, ILogger<PerioChartsController> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        // GET: api/PerioCharts/{patientId}/chart-data
        [HttpGet("{patientId}/chart-data")]
        public async Task<ActionResult<object>> GetPatientChartData(int patientId)
        {
            try
            {
                // Check if patient exists
                var patient = await _context.Patients.FindAsync(patientId);
                if (patient == null)
                {
                    return NotFound(new { message = "Patient not found" });
                }

                // Define the path for patient-specific chart data
                var chartsDirectory = Path.Combine(_env.ContentRootPath, "PerioCharts");
                var patientChartFile = Path.Combine(chartsDirectory, $"{patientId}.json");

                // If file doesn't exist, return default empty chart structure
                if (!System.IO.File.Exists(patientChartFile))
                {
                    return Ok(GetDefaultChartStructure());
                }

                // Read and return the patient's chart data
                var chartData = await System.IO.File.ReadAllTextAsync(patientChartFile);
                var parsedData = JsonSerializer.Deserialize<object>(chartData);
                return Ok(parsedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching chart data for patient {patientId}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // POST: api/PerioCharts/{patientId}/chart-data
        [HttpPost("{patientId}/chart-data")]
        public async Task<ActionResult<object>> SavePatientChartData(int patientId, [FromBody] object chartData)
        {
            try
            {
                // Check if patient exists
                var patient = await _context.Patients.FindAsync(patientId);
                if (patient == null)
                {
                    return NotFound(new { message = "Patient not found" });
                }

                // Create charts directory if it doesn't exist
                var chartsDirectory = Path.Combine(_env.ContentRootPath, "PerioCharts");
                if (!Directory.Exists(chartsDirectory))
                {
                    Directory.CreateDirectory(chartsDirectory);
                }

                // Save the chart data to patient-specific file
                var patientChartFile = Path.Combine(chartsDirectory, $"{patientId}.json");
                var jsonData = JsonSerializer.Serialize(chartData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await System.IO.File.WriteAllTextAsync(patientChartFile, jsonData);

                // Also update the database with a record of the chart
                var perioChart = await _context.PerioCharts
                    .Where(p => p.PatientId == patientId)
                    .OrderByDescending(p => p.Date)
                    .FirstOrDefaultAsync();

                if (perioChart == null)
                {
                    perioChart = new PerioChart
                    {
                        PatientId = patientId,
                        Date = DateTime.Now,
                        Notes = "Chart data saved via API"
                    };
                    _context.PerioCharts.Add(perioChart);
                }
                else
                {
                    perioChart.Date = DateTime.Now;
                    perioChart.Notes = "Chart data updated via API";
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Chart data saved successfully", patientId, timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving chart data for patient {patientId}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // DELETE: api/PerioCharts/{patientId}/chart-data
        [HttpDelete("{patientId}/chart-data")]
        public async Task<ActionResult<object>> ResetPatientChartData(int patientId)
        {
            try
            {
                // Check if patient exists
                var patient = await _context.Patients.FindAsync(patientId);
                if (patient == null)
                {
                    return NotFound(new { message = "Patient not found" });
                }

                // Define the path for patient-specific chart data
                var chartsDirectory = Path.Combine(_env.ContentRootPath, "PerioCharts");
                var patientChartFile = Path.Combine(chartsDirectory, $"{patientId}.json");

                // If file exists, delete it
                if (System.IO.File.Exists(patientChartFile))
                {
                    System.IO.File.Delete(patientChartFile);
                }

                // Save default empty chart structure
                var defaultData = GetDefaultChartStructure();
                var jsonData = JsonSerializer.Serialize(defaultData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await System.IO.File.WriteAllTextAsync(patientChartFile, jsonData);

                return Ok(new { message = "Chart data reset successfully", patientId, timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resetting chart data for patient {patientId}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        private object GetDefaultChartStructure()
        {
            // Return the default empty chart structure
            return new
            {
                tables = new[]
                {
                    new
                    {
                        name = "Table1Up",
                        position = "upper",
                        chart_surface = "Buccal",
                        teeth = GenerateDefaultTeethData("upper")
                    },
                    new
                    {
                        name = "Table1Down",
                        position = "lower",
                        chart_surface = "Buccal",
                        teeth = GenerateDefaultTeethData("lower")
                    },
                    new
                    {
                        name = "Table2Up",
                        position = "upper",
                        chart_surface = "Lingual",
                        teeth = GenerateDefaultTeethData("upper")
                    },
                    new
                    {
                        name = "Table2Down",
                        position = "lower",
                        chart_surface = "Lingual",
                        teeth = GenerateDefaultTeethData("lower")
                    }
                }
            };
        }

        private object[] GenerateDefaultTeethData(string position)
        {
            var teethData = new List<object>();

            // Define tooth IDs based on position
            int[] toothIds = position == "upper"
                ? new int[] { 18, 17, 16, 15, 14, 13, 12, 11, 21, 22, 23, 24, 25, 26, 27, 28 }
                : new int[] { 48, 47, 46, 45, 44, 43, 42, 41, 31, 32, 33, 34, 35, 36, 37, 38 };

            foreach (int toothId in toothIds)
            {
                teethData.Add(new
                {
                    id = toothId,
                    implant = false,
                    mobility = 0,
                    furcation = new { m = 0, d = 0, c = 0 },
                    bleed_on_probing = new { a = false, b = false, c = false },
                    plaque = new { a = false, b = false, c = false },
                    gingival_depth = new { a = 0, b = 0, c = 0 },
                    probing_depth = new { a = 0, b = 0, c = 0 }
                });
            }

            return teethData.ToArray();
        }
    }
}