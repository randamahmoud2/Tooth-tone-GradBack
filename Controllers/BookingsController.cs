using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DentalManagementAPI.Data;
using DentalManagementAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DentalManagementAPI.Models.DTOs;

namespace DentalManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(AppDbContext context, ILogger<BookingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Bookings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetBookings()
        {
            try
            {
                var bookings = await _context.Bookings
                    .Include(b => b.Patient)
                    .Include(b => b.Doctor)
                    .Select(b => new
                    {
                        id = b.Id,
                        doctorId = b.DoctorId,
                        patientId = b.PatientId,
                        bookingDate = b.BookingDate,
                        status = b.Status,
                        fee = b.Fee,
                        patientName = b.Patient.Name,
                        doctorName = b.Doctor.Name,
                        amount=b.PaymentAmount 
                    })
                    .ToListAsync();
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching bookings");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET: api/Bookings/doctor/{doctorId}/patients
        [HttpGet("doctor/{doctorId}/patients")]
        public async Task<ActionResult<IEnumerable<object>>> GetPatientsForDoctor(int doctorId)
        {
            try
            {
                var patients = await _context.Bookings
                    .Where(b => b.DoctorId == doctorId)
                    .Include(b => b.Patient)
                    .Select(b => new
                    {
                        id = b.Patient.Id,
                        name = b.Patient.Name,
                        age = b.Patient.Age,
                        gender = b.Patient.Gender,
                        national_id = b.Patient.NationalId ?? "N/A",
                        address = b.Patient.Address ?? "N/A"
                    })
                    .Distinct()
                    .ToListAsync();
                return Ok(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching patients for doctor");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/Bookings/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetBooking(int id)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Patient)
                    .Include(b => b.Doctor)
                    .Where(b => b.Id == id)
                    .Select(b => new
                    {
                        id = b.Id,
                        doctorId = b.DoctorId,
                        patientId = b.PatientId,
                        bookingDate = b.BookingDate,
                        status = b.Status,
                        fee = b.Fee,
                        patientName = b.Patient.Name,
                        doctorName = b.Doctor.Name
                    })
                    .FirstOrDefaultAsync();

                if (booking == null)
                {
                    return NotFound(new { message = "Booking not found" });
                }

                return Ok(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching booking");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST: api/Bookings (Taken from the second code)
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequest request)
        {
            try
            {
                
                if (request == null)
                {
                    return BadRequest(new { message = "Request body cannot be null" });
                }

                var patient = await _context.Patients.FindAsync(request.PatientId);
                if (patient == null)
                {
                    return NotFound(new { message = $"Patient with ID {request.PatientId} not found" });
                }

                var doctor = await _context.Doctors.FindAsync(request.DoctorId);
                if (doctor == null)
                {
                    return NotFound(new { message = $"Doctor with ID {request.DoctorId} not found" });
                }

                // التحقق من عدم وجود تعارض في المواعيد
                var existingBooking = await _context.Bookings
                    .Where(b => b.DoctorId == request.DoctorId)
                    .Where(b => b.BookingDate == request.BookingDate)
                    .FirstOrDefaultAsync();

                if (existingBooking != null)
                {
                    return Conflict(new
                    {
                        message = "Doctor already has a booking at this time",
                        existingBookingId = existingBooking.Id
                    });
                }

                var booking = new Booking
                {
                    PatientId = request.PatientId,
                    DoctorId = request.DoctorId,
                    BookingDate = request.BookingDate ?? DateTime.Now,
                    Status = "Pending",
                    Fee = doctor.Fee,
                    PatientName = patient.Name
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                
                return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, new
                {
                    message = "Booking created successfully",
                    bookingId = booking.Id,
                    patientName = patient.Name,
                    doctorName = doctor.Name,
                    bookingDate = booking.BookingDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    status = booking.Status,
                    fee = booking.Fee,
                    links = new
                    {
                        details = $"/api/Bookings/{booking.Id}",
                        updateStatus = $"/api/Bookings/{booking.Id}/status"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // DELETE: api/Bookings/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(id);
                if (booking == null)
                {
                    return NotFound(new { message = "Booking not found" });
                }

                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting booking");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // PUT: api/Bookings/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] string status)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(id);







                var appointment_date = booking!.Date;
                var appointment_patientid = booking!.PatientId;
                var doctor_id = booking!.DoctorId;




                var appointment = await _context.Appointments
           .FirstOrDefaultAsync(b => b.AppointmentDate == appointment_date
                                  && b.PatientId == appointment_patientid
                                  && b.DoctorId == doctor_id);
                


                


                if (booking == null)
                {
                    return NotFound(new { message = "Booking not found" });
                }


                booking.Status = status;
                appointment!.Status = status;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Booking status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking status");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET: api/Bookings/dashboard
        [HttpGet("dashboard")]
        public async Task<ActionResult<object>> GetDashboard()
        {
            try
            {
                var totalBookings = await _context.Bookings.CountAsync();
                var pendingBookings = await _context.Bookings.CountAsync(b => b.Status == "Pending");
                var confirmedBookings = await _context.Bookings.CountAsync(b => b.Status == "Confirmed");

                return Ok(new
                {
                    totalBookings,
                    pendingBookings,
                    confirmedBookings
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard data");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }

    public class BookingRequest
    {
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime? BookingDate { get; set; }
    }

    public class BookingStatusRequest
    {
        public string Status { get; set; }
    }
}