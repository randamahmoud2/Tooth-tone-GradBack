using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DentalManagementAPI.Models;
using DentalManagementAPI.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;

namespace DentalManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceptionistFinanceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReceptionistFinanceController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ReceptionistFinance/summary/{receptionistId}?period={period}
        [HttpGet("summary/{receptionistId}")]
        public async Task<IActionResult> GetSummary(int receptionistId, [FromQuery] string period = "month")
        {
            try
            {
                var query = _context.Salaries
                    .Where(s => s.ReceptionistId == receptionistId);

                // Apply period filter
                DateTime startDate;
                period = period?.ToLower() ?? "month";
                switch (period)
                {
                    case "week":
                        startDate = DateTime.Now.AddDays(-7);
                        query = query.Where(s => s.Date >= startDate);
                        break;
                    case "month":
                        startDate = DateTime.Now.AddMonths(-1);
                        query = query.Where(s => s.Date >= startDate);
                        break;
                    case "quarter":
                        startDate = DateTime.Now.AddMonths(-3);
                        query = query.Where(s => s.Date >= startDate);
                        break;
                    case "year":
                        startDate = DateTime.Now.AddYears(-1);
                        query = query.Where(s => s.Date >= startDate);
                        break;
                    case "all":
                        // No date filter for "all"
                        break;
                    default:
                        // Try parsing period as a month name (e.g., "June") or year (e.g., "2025")
                        if (DateTime.TryParseExact(period, "MMMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var monthDate))
                        {
                            int year = DateTime.Now.Year; // Use current year if not specified
                            startDate = new DateTime(year, monthDate.Month, 1);
                            var endDate = startDate.AddMonths(1);
                            query = query.Where(s => s.Date >= startDate && s.Date < endDate);
                        }
                        else if (int.TryParse(period, out int year))
                        {
                            startDate = new DateTime(year, 1, 1);
                            var endDate = startDate.AddYears(1);
                            query = query.Where(s => s.Date >= startDate && s.Date < endDate);
                        }
                        else
                        {
                            return BadRequest("Invalid period format. Use 'week', 'month', 'quarter', 'year', 'all', a year like '2025', or a month name like 'June'.");
                        }
                        break;
                }

                var salaries = await query.ToListAsync();

                var summary = new
                {
                    totalSalary = salaries.Sum(s => s.Amount),
                    basicSalary = salaries.Sum(s => s.BasicSalary),
                    allowances = salaries.Sum(s => s.Allowances),
                    netSalary = salaries.Sum(s => s.NetSalary)
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ReceptionistFinance/history/{receptionistId}
        [HttpGet("history/{receptionistId}")]
        public async Task<IActionResult> GetHistory(int receptionistId)
        {
            try
            {
                var history = await _context.Salaries
                    .Where(s => s.ReceptionistId == receptionistId)
                    .OrderByDescending(s => s.Date)
                    .Select(s => new
                    {
                        month = s.Month ?? s.Date.ToString("MMMM"),
                        date = s.Date.ToString("yyyy-MM-dd"),
                        amount = s.Amount,
                        status = s.Status // Assuming Status is already "Paid" or "Pending"
                    })
                    .ToListAsync();

                if (!history.Any())
                {
                    return NotFound($"No salary records found for receptionist with ID {receptionistId}.");
                }

                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}