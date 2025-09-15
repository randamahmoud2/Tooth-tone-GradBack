using DentalManagementAPI.Data;
using DentalManagementAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DentalManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HospitalLocation _hospitalLocation;

        public class HospitalLocation
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public int RadiusMeters { get; set; }
        }

        public AttendanceController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _hospitalLocation = configuration.GetSection("HospitalLocation").Get<HospitalLocation>() ?? new HospitalLocation
            {
                Latitude = 30.012258932723245, // Nile University center
                Longitude = 30.987065075068312,
                RadiusMeters = 2000 // 2000 meters radius
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
                return BadRequest("You must be at Nile University to check in");
            }

            // Check if user has already checked in today
            var today = DateTime.Today;
            var existingRecord = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a =>
                    a.UserId == request.UserId &&
                    a.UserType == request.UserType &&
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
                UserId = request.UserId,
                UserType = request.UserType, // "Doctor" or "Receptionist"
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
                Message = "Check-in recorded successfully"
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
                return BadRequest("You must be at Nile University to check out");
            }

            // Find the attendance record
            var record = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.Id == request.AttendanceRecordId && a.CheckOutTime == null);

            if (record == null)
            {
                return BadRequest("No active check-in record found");
            }

            // Update check-out time
            record.CheckOutTime = DateTime.Now;
            record.LocationCoordinates = $"{request.Latitude},{request.Longitude}";

            // Calculate working hours
            var workingHours = (record.CheckOutTime.Value - record.CheckInTime).TotalHours;
            record.WorkingHours = (decimal)Math.Round(workingHours, 2);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                record.Id,
                record.CheckOutTime,
                record.WorkingHours,
                Message = "Check-out recorded successfully"
            });
        }

        // GET: api/attendance/today/{userId}/{userType}
        [HttpGet("today/{userId}/{userType}")]
        public async Task<IActionResult> GetTodayStatus(int userId, string userType)
        {
            var today = DateTime.Today;
            var record = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a =>
                    a.UserId == userId &&
                    a.UserType == userType &&
                    a.Date.Date == today &&
                    a.CheckOutTime == null);

            if (record == null)
            {
                return Ok(new { isCheckedIn = false });
            }

            return Ok(new
            {
                isCheckedIn = true,
                record.Id,
                record.CheckInTime,
                record.Status
            });
        }

        // GET: api/attendance/history/{userId}/{userType}
        [HttpGet("history/{userId}/{userType}")]
        public async Task<IActionResult> GetHistory(int userId, string userType)
        {
            var records = await _context.AttendanceRecords
                .Where(a => a.UserId == userId && a.UserType == userType)
                .OrderByDescending(a => a.Date)
                .Select(a => new
                {
                    a.Id,
                    Date = a.Date.ToString("yyyy-MM-dd"),
                    CheckIn = a.CheckInTime.ToString("hh:mm tt"),
                    CheckOut = a.CheckOutTime.HasValue ? a.CheckOutTime.Value.ToString("hh:mm tt") : null,
                    a.Status,
                    WorkingHours = Math.Round(a.WorkingHours, 2),
                    a.LocationCoordinates
                })
                .ToListAsync();

            return Ok(records);
        }

        // Helper method to calculate distance using Haversine formula
        private bool IsWithinHospitalRadius(double lat1, double lon1, double lat2, double lon2, int radius)
        {
            const double EarthRadius = 6371000; // Earth's radius in meters
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = EarthRadius * c;
            return distance <= radius;
        }

        private double ToRadians(double deg)
        {
            return deg * Math.PI / 180;
        }

        public class CheckInRequest
        {
            public int UserId { get; set; }
            public string UserType { get; set; } // "Doctor" or "Receptionist"
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        public class CheckOutRequest
        {
            public int AttendanceRecordId { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }
    }
}