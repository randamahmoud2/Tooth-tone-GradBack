using DentalManagementAPI.Data;
using DentalManagementAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
namespace DentalManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HospitalLocation _hospitalLocation;

        public AttendanceController(AppDbContext context)
        {
            _context = context;
            _hospitalLocation = new HospitalLocation
            {
                Latitude = 30.0444,  // Cairo coordinates
                Longitude = 31.2357,
                RadiusMeters = 200
            };
        }

        // POST: api/attendance/checkin
        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInRequest request)
        {
            // Verify location
            var isWithinHospital = IsWithinHospitalRadius(
                request.Latitude,
                request.Longitude,
                _hospitalLocation.Latitude,
                _hospitalLocation.Longitude,
                _hospitalLocation.RadiusMeters);

            if (!isWithinHospital)
            {
                return BadRequest("You must be within hospital premises to check in");
            }

            // Check if already checked in today
            var today = DateTime.Today;
            var existingRecord = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a =>
                    a.DoctorId == request.DoctorId &&
                    a.Date.Date == today &&
                    a.CheckOutTime == null);

            if (existingRecord != null)
            {
                return BadRequest("You have already checked in today");
            }

            // Determine status
            var checkInTime = DateTime.Now;
            var status = "Present";

            var workdayStart = new DateTime(checkInTime.Year, checkInTime.Month, checkInTime.Day, 9, 0, 0);
            if (checkInTime > workdayStart.AddMinutes(15))
            {
                status = "Late";
            }

            // Create new record
            var record = new AttendanceRecord
            {
                DoctorId = request.DoctorId,
                Date = today,
                CheckInTime = checkInTime,
                Status = status,
                LocationCoordinates = $"{request.Latitude},{request.Longitude}",
                IsVerified = true
            };

            _context.AttendanceRecords.Add(record);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                record.Id,
                record.CheckInTime,
                record.Status,
                Message = "Checked in successfully"
            });
        }

        // POST: api/attendance/checkout
        [HttpPost("checkout")]
        public async Task<IActionResult> CheckOut([FromBody] CheckOutRequest request)
        {
            // Verify location
            var isWithinHospital = IsWithinHospitalRadius(
                request.Latitude,
                request.Longitude,
                _hospitalLocation.Latitude,
                _hospitalLocation.Longitude,
                _hospitalLocation.RadiusMeters);

            if (!isWithinHospital)
            {
                return BadRequest("You must be within hospital premises to check out");
            }

            // Get the record
            var record = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.Id == request.AttendanceRecordId);

            if (record == null)
            {
                return NotFound("Attendance record not found");
            }

            if (record.CheckOutTime != null)
            {
                return BadRequest("You have already checked out");
            }

            // Calculate working hours
            var checkOutTime = DateTime.Now;
            var workingHours = (decimal)(checkOutTime - record.CheckInTime).TotalHours;

            // Update record
            record.CheckOutTime = checkOutTime;
            record.WorkingHours = workingHours;
            record.LocationCoordinates = $"{request.Latitude},{request.Longitude}";
            record.IsVerified = true;

            _context.AttendanceRecords.Update(record);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                record.CheckOutTime,
                record.WorkingHours,
                Message = "Checked out successfully"
            });
        }

        // GET: api/attendance/history/{doctorId}
        [HttpGet("history/{doctorId}")]
        public async Task<IActionResult> GetAttendanceHistory(int doctorId, [FromQuery] int days = 30)
        {
            var cutoffDate = DateTime.Today.AddDays(-days);

            var history = await _context.AttendanceRecords
                .Where(a => a.DoctorId == doctorId && a.Date >= cutoffDate)
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.CheckInTime)
                .Select(a => new
                {
                    a.Id,
                    Date = a.Date.ToString("yyyy-MM-dd"),
                    CheckIn = a.CheckInTime.ToString("hh:mm tt"),
                    CheckOut = a.CheckOutTime.HasValue ? a.CheckOutTime.Value.ToString("hh:mm tt") : null,
                    a.Status,
                    WorkingHours = a.WorkingHours.ToString("0.00"),
                    Location = a.LocationCoordinates,
                    a.IsVerified
                })
                .ToListAsync();

            return Ok(history);
        }

        // GET: api/attendance/today/{doctorId}
        [HttpGet("today/{doctorId}")]
        public async Task<IActionResult> GetTodayStatus(int doctorId)
        {
            var today = DateTime.Today;

            var record = await _context.AttendanceRecords
                .Where(a => a.DoctorId == doctorId && a.Date == today)
                .OrderByDescending(a => a.CheckInTime)
                .FirstOrDefaultAsync();

            if (record == null)
            {
                return Ok(new
                {
                    IsCheckedIn = false,
                    Message = "Not checked in today"
                });
            }

            return Ok(new
            {
                IsCheckedIn = record.CheckOutTime == null,
                record.Id,
                record.CheckInTime,
                record.CheckOutTime,
                record.Status,
                record.WorkingHours,
                record.IsVerified
            });
        }

        // Helper method to check location
        private bool IsWithinHospitalRadius(double lat1, double lon1, double lat2, double lon2, int radius)
        {
            const double EarthRadius = 6371000; // meters
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = EarthRadius * c;

            return distance <= radius;
        }

        private double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}

