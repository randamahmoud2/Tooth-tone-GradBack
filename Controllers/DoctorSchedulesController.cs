using Microsoft.AspNetCore.Mvc;
using DentalManagementAPI.Data;
using DentalManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DentalManagementAPI.Controllers
{
    [Route("api/schedule")]
    [ApiController]
    public class DoctorScheduleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DoctorScheduleController(AppDbContext context)
        {
            _context = context;
        }


        [HttpPost("set")]
        public async Task<IActionResult> SetSchedule([FromBody] List<DoctorScheduleRequest> schedules)
        {
            if (schedules == null || !schedules.Any())
            {
                return BadRequest("No schedules provided");
            }

            // Get all doctor IDs from the request
            var doctorIds = schedules.Select(s => s.DoctorId).Distinct();

            // Check if all doctors exist
            var existingDoctors = await _context.Doctors
                .Where(d => doctorIds.Contains(d.Id))
                .Select(d => d.Id)
                .ToListAsync();

            var missingDoctors = doctorIds.Except(existingDoctors).ToList();
            if (missingDoctors.Any())
            {
                return BadRequest($"The following DoctorIds don't exist: {string.Join(", ", missingDoctors)}");
            }

            // Group schedules by doctor for batch processing
            var schedulesByDoctor = schedules.GroupBy(s => s.DoctorId);

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var doctorGroup in schedulesByDoctor)
                    {
                        var doctorId = doctorGroup.Key;

                        // **Option 1: Efficient way (EF Core 7.0+)**
                        await _context.DoctorSchedules
                            .Where(ds => ds.DoctorId == doctorId)
                            .ExecuteDeleteAsync();

                        // **Option 2: For older EF Core (if ExecuteDeleteAsync fails)**
                        // var existingSchedules = await _context.DoctorSchedules
                        //     .Where(ds => ds.DoctorId == doctorId)
                        //     .ToListAsync();
                        // _context.DoctorSchedules.RemoveRange(existingSchedules);

                        // Insert new schedules
                        var newSchedules = doctorGroup.Select(s => new DoctorSchedule
                        {
                            DoctorId = s.DoctorId,
                            DayOfWeek = s.DayOfWeek,
                            TimeSlot = s.TimeSlot,
                            IsAvailable = s.IsAvailable
                        }).ToList();

                        await _context.DoctorSchedules.AddRangeAsync(newSchedules);
                    }

                    // Save changes & commit transaction
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok("Schedule updated successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"Failed to save schedule: {ex.Message}");
                }
            }
        }





        // Doctor creates or updates their available schedule
        //[HttpPost("set")]
        //public IActionResult SetSchedule([FromBody] List<DoctorScheduleRequest> schedules)
        //{
        //    // Get all doctor IDs from the request
        //    var doctorIds = schedules.Select(s => s.DoctorId).Distinct();

        //    // Check if all doctors exist
        //    var existingDoctorIds = _context.Doctors
        //        .Where(d => doctorIds.Contains(d.Id))
        //        .Select(d => d.Id)
        //        .ToList();

        //    var missingDoctors = doctorIds.Except(existingDoctorIds).ToList();
        //    if (missingDoctors.Any())
        //    {
        //        return BadRequest($"The following DoctorIds don't exist: {string.Join(", ", missingDoctors)}");
        //    }

        //    foreach (var s in schedules)
        //    {
        //        var existing = _context.DoctorSchedules.FirstOrDefault(ds =>
        //            ds.DoctorId == s.DoctorId &&
        //            ds.DayOfWeek == s.DayOfWeek &&
        //            ds.TimeSlot == s.TimeSlot
        //        );

        //        if (existing != null)
        //        {
        //            existing.IsAvailable = s.IsAvailable;
        //            _context.DoctorSchedules.Update(existing);
        //        }
        //        else
        //        {
        //            var schedule = new DoctorSchedule
        //            {
        //                DoctorId = s.DoctorId,
        //                DayOfWeek = s.DayOfWeek,
        //                TimeSlot = s.TimeSlot,
        //                IsAvailable = s.IsAvailable
        //            };
        //            _context.DoctorSchedules.Add(schedule);
        //        }
        //    }

        //    _context.SaveChanges();
        //    return Ok("Schedule updated successfully.");
        //}




        // Get available slots for a doctor
        [HttpGet("available/{doctorId}")]
        public IActionResult GetAvailableSlots(int doctorId)
        {
            var availableSlots = _context.DoctorSchedules
                .Where(s => s.DoctorId == doctorId && s.IsAvailable)
                .ToList();

            return Ok(availableSlots);
        }
    }

    public class DoctorScheduleRequest
    {
        public int DoctorId { get; set; }
        public int DayOfWeek { get; set; } // 0=Sunday ... 6=Saturday
        public string TimeSlot { get; set; } // e.g., "10:00 AM"
        public bool IsAvailable { get; set; }
    }
}
