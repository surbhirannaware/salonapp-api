using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonApp.Domain.Entities;
using static System.Reflection.Metadata.BlobBuilder;

namespace SalonApp.Controllers
{
    [ApiController]
    [Route("api/availability")]
    public class AvailabilityController : ControllerBase
    {
        private readonly SalonDbContext _db;

        public AvailabilityController(SalonDbContext db)
        {
            _db = db;
        }

        [HttpGet("slots")]
        public async Task<IActionResult> GetUnionSlots(
      DateTime date,
      [FromQuery] List<int> serviceIds)
        {
            if (serviceIds == null || !serviceIds.Any())
                return Ok(new List<object>());

            // 1️⃣ Get selected services
            var services = await _db.Services
                .Where(s => serviceIds.Contains(s.ServiceId) && s.IsActive)
                .ToListAsync();

            if (services.Count != serviceIds.Count)
                return BadRequest("Invalid services");

            int totalDuration = services.Sum(s => s.DurationMinutes);

            var allSlots = new List<TimeSpan>();

            // 2️⃣ Get eligible staff
            var staffList = await _db.Staff
                .Where(s => s.IsActive)
                .Include(s => s.StaffServices)
                .ToListAsync();

            foreach (var staff in staffList)
            {
                var staffServiceIds = staff.StaffServices
                    .Select(s => s.ServiceId)
                    .ToHashSet();

                // Staff must support all selected services
                if (!serviceIds.All(id => staffServiceIds.Contains(id)))
                    continue;

                // 3️⃣ Staff availability
                var availability = await _db.StaffAvailabilities
                    .FirstOrDefaultAsync(a =>
                        a.StaffId == staff.StaffId &&
                        a.DayOfWeek == (int)date.DayOfWeek &&
                        a.IsActive);

                if (availability == null)
                    continue;

                // 4️⃣ Check leave
                var onLeave = await _db.StaffLeaves
                    .AnyAsync(l =>
                        l.StaffId == staff.StaffId &&
                        l.LeaveDate == date.Date);

                if (onLeave)
                    continue;

                // 5️⃣ Get appointments
                var appointments = await _db.Appointments
                    .Where(a =>
                        a.StaffId == staff.StaffId &&
                        a.AppointmentDate == date.Date &&
                        a.Status != "Cancelled")
                    .ToListAsync();

                var start = availability.StartTime;
                var end = availability.EndTime;

                while (start + TimeSpan.FromMinutes(totalDuration) <= end)
                {
                    var slotEnd = start + TimeSpan.FromMinutes(totalDuration);

                    bool overlap = appointments.Any(a =>
                        start < a.EndTime && slotEnd > a.StartTime);

                    if (!overlap)
                        allSlots.Add(start);

                    // 30 minute slot interval
                    start = start.Add(TimeSpan.FromMinutes(30));
                }
            }

            // 6️⃣ Remove past slots if today
            if (date.Date == DateTime.Today)
            {
                var now = DateTime.Now.TimeOfDay;
                allSlots = allSlots
                    .Where(s => s > now)
                    .ToList();
            }

            // 7️⃣ Final response
            var result = allSlots
                .Distinct()
                .OrderBy(s => s)
                .Select(s => new
                {
                    startTime = s.ToString(@"hh\:mm"),
                    endTime = s.Add(TimeSpan.FromMinutes(totalDuration))
                               .ToString(@"hh\:mm")
                });

            return Ok(result);
        }
    }
}