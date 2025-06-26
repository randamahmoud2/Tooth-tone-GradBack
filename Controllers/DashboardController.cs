using DentalManagementAPI.Data;
using DentalManagementAPI.Models;
using DentalManagementAPI.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DentalManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }


        public static string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToUpper(input[0]) + input.Substring(1);
        }


        // GET: api/dashboard/stats/{doctorId}
        [HttpGet("stats/{doctorId}")]
        public async Task<IActionResult> GetDashboardStats(int doctorId)
        {
            var stats = new DashboardStats();

            // Calculate total earnings (only paid bookings)
            stats.TotalEarnings = await _context.Bookings
                .Where(b => b.DoctorId == doctorId && b.IsPaid)
                .SumAsync(b => b.PaymentAmount);

            // Count all appointments
            stats.AppointmentsCount = await _context.Bookings
                .Where(b => b.DoctorId == doctorId)
                .CountAsync();

            // Count unique patients
            stats.PatientsCount = await _context.Bookings
                .Where(b => b.DoctorId == doctorId)
                .Select(b => b.PatientId)
                .Distinct()
                .CountAsync();

            // Get recent bookings (last 5)
            stats.RecentBookings = await _context.Bookings
                .Where(b => b.DoctorId == doctorId)
                .OrderByDescending(b => b.Date)
                .ThenByDescending(b => b.Time)
                .Take(5)
                .ToListAsync();

            return Ok(stats);
        }

        // GET: api/dashboard/bookings/{doctorId}
        [HttpGet("bookings/{doctorId}")]
        public async Task<IActionResult> GetBookings(int doctorId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.DoctorId == doctorId)
                .OrderByDescending(b => b.Date)
                .ThenByDescending(b => b.Time)
                .ToListAsync();

            return Ok(bookings);
        }

        // PUT: api/dashboard/booking/status
        [HttpPut("booking/status")]
        public async Task<IActionResult> UpdateBookingStatus([FromBody] UpdateBookingStatusRequest request)
        {
            var booking = await _context.Bookings.FindAsync(request.BookingId);

            var appointment_date = booking!.Date;
            var appointment_patientid = booking!.PatientId;
            var doctor_id = booking!.DoctorId;



            if (booking == null)
            {
                return NotFound("Booking not found");
            }

            if (request.Status != "completed" && request.Status != "cancelled")
            {
                return BadRequest("Invalid status value");
            }

            var appointment = await _context.Appointments
          .FirstOrDefaultAsync(b => b.AppointmentDate == appointment_date
                                 && b.PatientId == appointment_patientid
                                 && b.DoctorId == doctor_id);




            booking.Status = CapitalizeFirstLetter(request.Status);
            appointment!.Status = CapitalizeFirstLetter(request.Status);

            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();








            if (request.Status.ToLower() == "paid")
            {
                booking.IsPaid = true;
            }
            else
            {
                booking.IsPaid = false;
            }










            return Ok(new
            {
                Message = "Booking status updated successfully",
                BookingId = booking.Id,
                NewStatus = booking.Status
            });
        }























        [HttpGet("GetDoctorsEarning/{doctorId}")]
        public async Task<IActionResult> GetDoctorsEarning(int doctorId)
        {
            try
            {
                // Get the UserId associated with the provided DoctorId
                var userId = await _context.Doctors
                    .Where(d => d.Id == doctorId)
                    .Select(d => d.UserId)
                    .FirstOrDefaultAsync();

                // Check if doctor exists
                if (userId == 0)
                {
                    return
                        NotFound("Doctor not found.");
                }

                // Query the DoctorsEarnings table using the UserId
                var query = @"
            SELECT [UserId], [Account], [Name], [TotalEarning]
            FROM [DentalDB].[dbo].[DoctorsEarnings]
            WHERE [UserId] = @userId";

                var results = await _context.Set<DoctorEarningDTO>()
                    .FromSqlRaw(query, new SqlParameter("@userId", userId))
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(results);
            }
            catch (Exception ex)
            {
                // Log exception here if needed
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }







        [HttpGet("GetAdminEarning")]
        public async Task<IActionResult> GetAdminEarning()
        {
            var query = @"
        SELECT Id, TotalEarningsFees
        FROM [DentalDB].[dbo].[AdminEarnings];";

            var results = await _context
                .AdminEarnings
                .FromSqlRaw(query)
                .AsNoTracking()
                .ToListAsync();

            return Ok(results);
        }



































    }
}
    

