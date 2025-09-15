using Microsoft.AspNetCore.Mvc;
using DentalManagementAPI.Data;
using DentalManagementAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DentalManagementAPI.Models.DTOs;

[Route("api/[controller]")]
[ApiController]
public class AppointmentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    private readonly ILogger _logger;



    public AppointmentsController(AppDbContext context, IMapper mapper, ILoggerFactory loggerFactory)
    {
        _context = context;
        _mapper = mapper;
        _logger = loggerFactory.CreateLogger("AppointmentsController");



    }

    [HttpPost]
    public async Task<ActionResult<AppointmentResponseDto>> CreateAppointment([FromBody] AppointmentDTO appointmentDto)
    {
        try
        {
            _logger.LogInformation("This error message should definitely appear");
            if (!ModelState.IsValid)
            {
               
                return BadRequest(ModelState);
            }

            // Use AsNoTracking to prevent tracking conflicts
            var patient = await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == appointmentDto.PatientId);

            var doctor = await _context.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == appointmentDto.DoctorId);

            if (patient == null)
            {
                return BadRequest(new { message = $"Patient with ID {appointmentDto.PatientId} not found" });
            }

            if (doctor == null)
            {
                return BadRequest(new { message = $"Doctor with ID {appointmentDto.DoctorId} not found" });
            }

            // Check if time slot is available
            var isSlotTaken = await _context.Appointments
                .AnyAsync(a => a.DoctorId == appointmentDto.DoctorId &&
                              a.AppointmentDate.Date == appointmentDto.AppointmentDate.Date &&
                              a.TimeSlot == appointmentDto.TimeSlot &&
                              a.Status != "Cancelled");

            if (isSlotTaken)
            {
                return Conflict(new { message = "This time slot is already booked" });
            }

            // Create appointment without mapping to avoid tracking issues
            var appointment = new Appointment
            {
                PatientId = appointmentDto.PatientId,
                DoctorId = appointmentDto.DoctorId,
                AppointmentDate = appointmentDto.AppointmentDate,
                TimeSlot = appointmentDto.TimeSlot,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending"
            };
            _context.Appointments.Add(appointment);

            // Create booking without mapping DTOs to entities
            var booking = new Booking
            {
                PatientId = appointmentDto.PatientId,
                PatientName = patient.Name,
                Date = appointmentDto.AppointmentDate,
                Time = DateTime.Parse(appointmentDto.TimeSlot).TimeOfDay,
                DoctorId = appointmentDto.DoctorId,
                Status = "Pending",
                BookingDate = DateTime.UtcNow,
                Fee = doctor.Fee,
                PaymentAmount = doctor.Fee,
                // Don't attach navigation properties here to prevent tracking conflicts
                Patient = null,
                Doctor = null
            };
            _context.Bookings.Add(booking);

            await _context.SaveChangesAsync();
            //Console.WriteLine(patient.);
            // Reload appointment with related data for response
            var createdAppointment = await _context.Appointments
                .AsNoTracking() // Important to prevent tracking conflicts
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == appointment.Id);

            if (createdAppointment == null)
            {
                return NotFound();
            }

            // Map to DTO for response
            var responseDto = new AppointmentResponseDto
            {
                Id = createdAppointment.Id,
                PatientId = createdAppointment.PatientId,
                DoctorId = createdAppointment.DoctorId,
                AppointmentDate = createdAppointment.AppointmentDate,
                TimeSlot = createdAppointment.TimeSlot,
                CreatedAt = createdAppointment.CreatedAt,
                Status = createdAppointment.Status,
                Patient = _mapper.Map<PatientDto>(createdAppointment.Patient),
                Doctor = _mapper.Map<DoctorDto>(createdAppointment.Doctor)
            };

            return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, responseDto);
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new { message = "Error saving appointment", error = ex.InnerException?.Message ?? ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }











    [HttpGet("{id}")]
    public async Task<ActionResult<Appointment>> GetAppointment(int id)
    {
        try
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound(new { message = "Appointment not found" });
            }

            return Ok(appointment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    // GET: api/Appointments/patient/{patientId}
    [HttpGet("patient/{patientId}")]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointmentsByPatient(int patientId)
    {
        try
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.TimeSlot)
                .ToListAsync();

            if (!appointments.Any())
            {
                return NotFound(new { message = "No appointments found for this patient" });
            }

            return Ok(appointments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    // DELETE: api/Appointments/{id}
    //[HttpDelete("{id}")]
    //public async Task<IActionResult> CancelAppointment(int id)
    //{
    //    try
    //    {
    //        var appointment = await _context.Appointments
    //            .Include(a => a.Patient)
    //            .Include(a => a.Doctor)
    //            .FirstOrDefaultAsync(a => a.Id == id);



    //        var appointment_date = appointment!.AppointmentDate;
    //        var appointment_patientid = appointment!.PatientId;
    //        var doctor_id = appointment!.DoctorId;

    //        if (appointment == null)
    //        {
    //            return NotFound(new { message = "Appointment not found" });
    //        }

    //        // Soft delete by changing status
    //        appointment.Status = "Cancelled";
    //        appointment.CreatedAt = DateTime.UtcNow;
    //        var booking = await _context.Bookings
    //            .FirstOrDefaultAsync(b => b.Date == appointment_date
    //                                   && b.PatientId == appointment_patientid
    //                                   && b.DoctorId == doctor_id);
    //        booking!.Status = appointment.Status;
    //        await _context.SaveChangesAsync();

    //        return Ok(new
    //        {
    //            message = "Appointment cancelled successfully",
    //            appointment = appointment
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine(ex);

    //        return StatusCode(500, new { message = "Error cancelling appointment", error = ex.Message });
    //    }
    //}




    // DELETE: api/Appointments/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelAppointment(int id)
    {
        try
        {
            // Get appointment with necessary includes
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound(new { message = "Appointment not found" });
            }

            // Update appointment status
            appointment.Status = "Cancelled";
            appointment.CreatedAt = DateTime.UtcNow;

            // Find and update related booking
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Date == appointment.AppointmentDate
                                       && b.PatientId == appointment.PatientId
                                       && b.DoctorId == appointment.DoctorId);

            if (booking != null)
            {
                booking.Status = appointment.Status;
            }

            await _context.SaveChangesAsync();

            // Map to DTOs
            var appointmentDto = new AppointmentDTO
            {
                Id = appointment.Id,
                PatientId = appointment.PatientId,
                DoctorId = appointment.DoctorId,
                AppointmentDate = appointment.AppointmentDate,
                TimeSlot = appointment.TimeSlot,
                CreatedAt = appointment.CreatedAt,
                Status = appointment.Status,
                Patient = appointment.Patient != null ? new PatientDto
                {
                    // Only include necessary patient properties
                    Id = appointment.Patient.Id,
                    Name = appointment.Patient.Name,
                    // Add other needed properties
                } : null,
                Doctor = appointment.Doctor != null ? new Doctor
                {
                    // Only include necessary doctor properties
                    Id = appointment.Doctor.Id,
                    Name = appointment.Doctor.Name,
                    // Add other needed properties
                } : null
            };

            var bookingDto = booking != null ? new BookingResponseDto
            {
                Id = booking.Id,
                Date = booking.Date,
                Time = booking.Time,
                Status = booking.Status,
                Patient = booking.Patient != null ? new PatientDto
                {
                    Id = booking.Patient.Id,
                    Name = booking.Patient.Name,
                } : null,
                Doctor = booking.Doctor != null ? new DoctorDto
                {
                    Id = booking.Doctor.Id,
                    Name = booking.Doctor.Name,
                } : null
            } : null;

            return Ok(new
            {
                message = "Appointment cancelled successfully",
                appointment = appointmentDto,
                booking = bookingDto
            });
        }
        catch (Exception ex)
        {
            // In production, use proper logging instead of Console.WriteLine
            return StatusCode(500, new
            {
                message = "Error cancelling appointment",
                error = ex.Message
            });
        }
    }










    [HttpGet]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetAllAppointments()
    {
        try
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.TimeSlot)
                .ToListAsync();

            return Ok(appointments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }







    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateAppointmentStatus(int id, [FromBody] StatusUpdateDto statusUpdate)
    {
        try
        {
            var appointment = await _context.Appointments.FindAsync(id);
           
   
            if (appointment == null)
            {
                return NotFound(new { message = "Appointment not found" });
            }



            var appointment_date = appointment!.AppointmentDate;
            var appointment_patientid = appointment!.PatientId;
            var doctor_id = appointment!.DoctorId;


            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Date == appointment_date
                                       && b.PatientId == appointment_patientid
                                       && b.DoctorId == doctor_id);

            booking!.Status = statusUpdate.Status;


            // Update status (e.g., 'Completed' or 'Active')
            appointment.Status = statusUpdate.Status;

            if (statusUpdate.Status.ToLower() == "paid")
            {
                booking.IsPaid = true;
            }
            else
            {
                booking.IsPaid = false;
            }




                await _context.SaveChangesAsync();

            return Ok(new { message = "Status updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }














































    [HttpPatch("{id}/paid")]
    public async Task<IActionResult> MarkAppointmentAsPaid(int id)
    {
        try
        {
            // Find the appointment
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound(new { message = "Appointment not found" });
            }

            // Update the paid status
            appointment.Status = "Paid";
            await _context.SaveChangesAsync();

            // Find the associated booking (if needed)
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.PatientId == appointment.PatientId &&
                                        b.DoctorId == appointment.DoctorId &&
                                        b.Date == appointment.AppointmentDate);

            if (booking != null)
            {
                booking.Status = "Paid";
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Appointment marked as paid successfully",
                appointment = appointment
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating payment status", error = ex.Message });
        }
    }






























}