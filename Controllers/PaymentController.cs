using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DentalManagementAPI.Models;
using DentalManagementAPI.Data;

namespace DentalManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Temporary dictionary to map PatientId to Procedure Name
        private static readonly Dictionary<int, string> PatientProcedureMap = new Dictionary<int, string>
        {
            { 5, "Zirconia Crown" }, // reem ahmed
            { 8, "Composite Filling" }, // Sara Esalam
            { 6, "Extraction" }, // nouran galal
            { 10, "Amalgam Filling" }, // ali
            { 7, "Root Canal Treatment" }, // Ali ahmed
            { 12, "Professional Cleaning" } // Saif
            // Add more mappings as needed
        };



        public PaymentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Payment
        [HttpGet]
        public async Task<IActionResult> GetAllPayments()
        {
            try
            {
                var payments = await _context.Bookings
                    .Include(b => b.Doctor)
                    .Where(b => b.IsPaid)
                    .OrderByDescending(b => b.Date)
                    .Select(b => new
                    {
                        id = b.Id,
                        date = b.Date.ToString("yyyy-MM-dd"),
                        procedure = "Dental Procedure", // يمكن استبدالها بحقل حقيقي من الداتابيز
                        doctor = b.Doctor != null ? $" {b.Doctor.Name}" : "N/A",
                        amount = b.PaymentAmount,
                        status = b.Status
                    })
                    .ToListAsync();

                if (!payments.Any())
                {
                    return NotFound("No paid bookings found.");
                }

                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Payment/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetPatientPayments(int patientId)
        {
            try
            {
                var payments = await _context.Bookings
                    .Include(b => b.Doctor)
                    .Where(b => b.PatientId == patientId) // إزالة شرط b.IsPaid
                    .OrderByDescending(b => b.Date)
                    .Select(b => new
                    {
                        id = b.Id,
                        date = b.Date.ToString("yyyy-MM-dd"),
                        procedure = "Dental Procedure", // يمكن استبدالها بحقل حقيقي
                        doctor = b.Doctor != null ? $" {b.Doctor.Name}" : "N/A",
                        amount = b.PaymentAmount,
                        status = b.Status
                    })
                    .ToListAsync();

                if (!payments.Any())
                {
                    return NotFound($"No bookings found for patient with ID {patientId}.");
                }

                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Payment/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(int id)
        {
            try
            {
                var payment = await _context.Bookings
                    .Include(b => b.Doctor)
                    .Where(b => b.Id == id && b.IsPaid)
                    .Select(b => new
                    {
                        id = b.Id,
                        date = b.Date.ToString("yyyy-MM-dd"),
                        procedure = "Dental Procedure",
                        doctor = b.Doctor != null ? $". {b.Doctor.Name}" : "N/A",
                        amount = b.PaymentAmount,
                        status = b.Status
                    })
                    .FirstOrDefaultAsync();

                if (payment == null)
                {
                    return NotFound($"Payment with ID {id} not found or not paid.");
                }

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}