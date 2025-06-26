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
    public class DoctorFinanceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DoctorFinanceController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DoctorFinance/summary/{doctorId}?period={period}
        [HttpGet("summary/{doctorId}")]
        public async Task<IActionResult> GetSummary(int doctorId, [FromQuery] string period = "month")
        {
            try
            {
                var query = _context.Bookings
                    .Include(b => b.Doctor)
                    .Where(b => b.DoctorId == doctorId);

                // Apply period filter
                DateTime startDate;
                period = period?.ToLower() ?? "month";
                switch (period)
                {
                    case "week":
                        startDate = DateTime.Now.AddDays(-7);
                        query = query.Where(b => b.Date >= startDate);
                        break;
                    case "month":
                        startDate = DateTime.Now.AddMonths(-1);
                        query = query.Where(b => b.Date >= startDate);
                        break;
                    case "quarter":
                        startDate = DateTime.Now.AddMonths(-3);
                        query = query.Where(b => b.Date >= startDate);
                        break;
                    case "year":
                        startDate = DateTime.Now.AddYears(-1);
                        query = query.Where(b => b.Date >= startDate);
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
                            query = query.Where(b => b.Date >= startDate && b.Date < endDate);
                        }
                        else if (int.TryParse(period, out int year))
                        {
                            startDate = new DateTime(year, 1, 1);
                            var endDate = startDate.AddYears(1);
                            query = query.Where(b => b.Date >= startDate && b.Date < endDate);
                        }
                        else
                        {
                            return BadRequest("Invalid period format. Use 'week', 'month', 'quarter', 'year', 'all', a year like '2025', or a month name like 'June'.");
                        }
                        break;
                }

                var bookings = await query.ToListAsync();

                var summary = new
                {
                    totalEarnings = bookings.Sum(b => b.PaymentAmount),
                    pendingPayments = bookings.Where(b => !b.IsPaid).Sum(b => b.PaymentAmount),
                    completedPayments = bookings.Where(b => b.IsPaid).Sum(b => b.PaymentAmount),
                    totalPatients = bookings.Select(b => b.PatientId).Distinct().Count(),
                    averagePerPatient = bookings.Any() ? bookings.Sum(b => b.PaymentAmount) / bookings.Select(b => b.PatientId).Distinct().Count() : 0
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/DoctorFinance/payments/{doctorId}
        [HttpGet("payments/{doctorId}")]
        public async Task<IActionResult> GetPayments(int doctorId)
        {
            try
            {
                var payments = await _context.Bookings
                    .Include(b => b.Doctor)
                    .Where(b => b.DoctorId == doctorId)
                    .OrderByDescending(b => b.Date)
                    .Select(b => new
                    {
                        id = b.Id,
                        patientName = b.PatientName ?? "Unknown",
                        service = "Dental Procedure", // يمكن استبدالها بحقل حقيقي إذا كان موجود
                        date = b.Date.ToString("yyyy-MM-dd"),
                        amount = b.PaymentAmount,
                        status = b.IsPaid ? "Completed" : "Pending" // Match frontend expectation
                    })
                    .ToListAsync();

                if (!payments.Any())
                {
                    return NotFound($"No bookings found for doctor with ID {doctorId}.");
                }

                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/DoctorFinance/analytics/{doctorId}?period={period}
        [HttpGet("analytics/{doctorId}")]
        public async Task<IActionResult> GetAnalytics(int doctorId, [FromQuery] string period = "year")
        {
            try
            {
                var query = _context.Bookings
                    .Where(b => b.DoctorId == doctorId);

                // Apply period filter
                DateTime startDate;
                period = period?.ToLower() ?? "year";
                switch (period)
                {
                    case "week":
                        startDate = DateTime.Now.AddDays(-7);
                        query = query.Where(b => b.Date >= startDate);
                        break;
                    case "month":
                        startDate = DateTime.Now.AddMonths(-1);
                        query = query.Where(b => b.Date >= startDate);
                        break;
                    case "quarter":
                        startDate = DateTime.Now.AddMonths(-3);
                        query = query.Where(b => b.Date >= startDate);
                        break;
                    case "year":
                        startDate = DateTime.Now.AddYears(-1);
                        query = query.Where(b => b.Date >= startDate);
                        break;
                    case "all":
                        // No date filter for "all"
                        break;
                    default:
                        // Try parsing period as a year (e.g., "2025") or month (e.g., "June")
                        if (int.TryParse(period, out int year))
                        {
                            startDate = new DateTime(year, 1, 1);
                            var endDate = startDate.AddYears(1);
                            query = query.Where(b => b.Date >= startDate && b.Date < endDate);
                        }
                        else if (DateTime.TryParseExact(period, "MMMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var monthDate))
                        {
                            int yearValue = DateTime.Now.Year; // Use current year if not specified
                            startDate = new DateTime(yearValue, monthDate.Month, 1);
                            var endDate = startDate.AddMonths(1);
                            query = query.Where(b => b.Date >= startDate && b.Date < endDate);
                        }
                        else
                        {
                            return BadRequest("Invalid period format. Use 'week', 'month', 'quarter', 'year', 'all', a year like '2025', or a month name like 'June'.");
                        }
                        break;
                }

                var bookings = await query.ToListAsync();

                // Monthly Earnings
                var monthlyEarnings = Enumerable.Range(0, 12)
                    .Select(i => new
                    {
                        month = DateTime.Now.AddMonths(-11 + i).ToString("MMM"),
                        amount = bookings
                            .Where(b => b.Date.Year == DateTime.Now.AddMonths(-11 + i).Year &&
                                        b.Date.Month == DateTime.Now.AddMonths(-11 + i).Month)
                            .Sum(b => b.PaymentAmount)
                    })
                    .ToList();

                // Service Breakdown
                var serviceBreakdown = bookings
                    .GroupBy(b => "Dental Procedure") // يمكن استبدالها بحقل حقيقي
                    .Select(g => new
                    {
                        service = g.Key,
                        count = g.Count(),
                        revenue = g.Sum(b => b.PaymentAmount),
                        percentage = bookings.Any() ? (g.Sum(b => b.PaymentAmount) / bookings.Sum(b => b.PaymentAmount) * 100) : 0
                    })
                    .ToList();

                return Ok(new
                {
                    monthlyEarnings,
                    serviceBreakdown
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}