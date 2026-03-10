//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using SalonApp.Controllers.DTOs;
//using SalonApp.Domain.Entities;

//namespace SalonApp.Controllers
//{
//    [ApiController]
//    [Route("api/availability")]
//    public class DiscardedAvailabilityController : ControllerBase
//    {
//        private readonly SalonDbContext _db;

//        public DiscardedAvailabilityController(SalonDbContext db)
//        {
//            _db = db;
//        }

//        /* =========================================================
//           1️⃣ GET AVAILABLE STAFF (Service + Date)
//        ========================================================= */

//        //[HttpGet("staff")]
//        //public async Task<IActionResult> GetAvailableStaff(
//        //    [FromQuery] DateTime date,
//        //    [FromQuery] List<int> serviceIds)
//        //{
//        //    if (serviceIds == null || !serviceIds.Any())
//        //        return Ok(new List<object>());

//        //    int dayOfWeek = (int)date.DayOfWeek;

//        //    var services = await _db.Services
//        //        .Where(s => serviceIds.Contains(s.ServiceId))
//        //        .ToListAsync();

//        //    if (services.Count != serviceIds.Count)
//        //        return BadRequest("Invalid services selected.");

//        //    int totalDuration = services.Sum(s => s.DurationMinutes);

//        //    var eligibleStaff = await _db.Staff
//        //        .Where(s => s.IsActive)
//        //        .Where(s =>
//        //            s.StaffServices
//        //                .Where(ss => serviceIds.Contains(ss.ServiceId))
//        //                .Select(ss => ss.ServiceId)
//        //                .Distinct()
//        //                .Count() == serviceIds.Count)
//        //        .Select(s => new
//        //        {
//        //            staffId = s.StaffId,
//        //            staffName = s.User.FullName
//        //        })
//        //        .ToListAsync();

//        //    var availableStaffList = new List<object>();

//        //    foreach (var staff in eligibleStaff)
//        //    {
//        //        Console.WriteLine(dayOfWeek);

//        //        var availability = await _db.StaffAvailabilities
//        //            .FirstOrDefaultAsync(a =>
//        //                a.StaffId == staff.staffId &&
//        //                a.DayOfWeek == dayOfWeek &&
//        //                a.IsActive);

//        //        if (availability == null)
//        //            continue;

//        //        var onLeave = await _db.StaffLeaves
//        //            .AnyAsync(l =>
//        //                l.StaffId == staff.staffId &&
//        //                l.LeaveDate.Date == date.Date);
                 

//        //        if (onLeave)
//        //            continue;

//        //        var appointments = await _db.Appointments
//        //            .Where(a =>
//        //                a.StaffId == staff.staffId &&
//        //                a.AppointmentDate == date.Date &&
//        //                a.Status != "Cancelled")
//        //            .Select(a => new BookedSlotDto
//        //            {
//        //                StartTime = a.StartTime,
//        //                EndTime = a.EndTime
//        //            })
//        //            .ToListAsync();

//        //        var slots = BuildSlots(
//        //            date.Date,
//        //            availability.StartTime,
//        //            availability.EndTime,
//        //            totalDuration,
//        //            appointments,
//        //            new List<StaffLeave>()
//        //        );

//        //       // if (slots.Any())
//        //       if(true)
//        //        {
//        //            availableStaffList.Add(new
//        //            {
//        //                staffId = staff.staffId,
//        //                staffName = staff.staffName
//        //            });
//        //        }
//        //    }

//        //    return Ok(availableStaffList);
//        //}

//        /* =========================================================
//           2️⃣ GET STAFF SLOTS (Service + Date + Staff)
//        ========================================================= */

//        [HttpGet("staff/{staffId}")]
//        public async Task<IActionResult> GetStaffAvailability(
//            int staffId,
//            [FromQuery] DateTime date,
//            [FromQuery] List<int> serviceIds)
//        {
//            var staff = await _db.Staff
//                .Include(s => s.StaffServices)
//                .FirstOrDefaultAsync(s => s.StaffId == staffId && s.IsActive);

//            if (staff == null)
//                return BadRequest("Invalid staff");

//            var services = await _db.Services
//                .Where(s => serviceIds.Contains(s.ServiceId))
//                .ToListAsync();

//            if (services.Count != serviceIds.Count)
//                return BadRequest("Invalid services");

//            var staffServiceIds = staff.StaffServices
//                .Select(s => s.ServiceId)
//                .ToHashSet();

//            if (!serviceIds.All(id => staffServiceIds.Contains(id)))
//                return BadRequest("Staff does not provide selected services");

//            int totalDuration = services.Sum(s => s.DurationMinutes);
//            int dayOfWeek = (int)date.DayOfWeek;

//            var availability = await _db.StaffAvailabilities
//                .FirstOrDefaultAsync(a =>
//                    a.StaffId == staffId &&
//                    a.DayOfWeek == dayOfWeek &&
//                    a.IsActive);

//            if (availability == null)
//                return Ok(new List<TimeSlotResponse>());

//            var appointments = await _db.Appointments
//                .Where(a =>
//                    a.StaffId == staffId &&
//                    a.AppointmentDate == date.Date &&
//                    a.Status != "Cancelled")
//                .Select(a => new BookedSlotDto
//                {
//                    StartTime = a.StartTime,
//                    EndTime = a.EndTime
//                })
//                .ToListAsync();

//            var leaves = await _db.StaffLeaves
//                .Where(l =>
//                    l.StaffId == staffId &&
//                    l.LeaveDate == date.Date)
//                .ToListAsync();

//            var slots = BuildSlots(
//                date.Date,
//                availability.StartTime,
//                availability.EndTime,
//                totalDuration,
//                appointments,
//                leaves
//            );

//            return Ok(slots);
//        }

//        /* =========================================================
//            GET STAFF SLOTS (Service + Date + Staff)
//        ========================================================= */


//    //    [HttpGet("slots")]
//    //    public async Task<IActionResult> GetUnionSlots(
//    //[FromQuery] DateTime date,
//    //[FromQuery] List<int> serviceIds)
//    //    {
//    //        if (serviceIds == null || !serviceIds.Any())
//    //            return Ok(new List<object>());

//    //        int dayOfWeek = (int)date.DayOfWeek;

//    //        var services = await _db.Services
//    //            .Where(s => serviceIds.Contains(s.ServiceId))
//    //            .ToListAsync();

//    //        if (services.Count != serviceIds.Count)
//    //            return BadRequest("Invalid services");

//    //        int totalDuration = services.Sum(s => s.DurationMinutes);

//    //        var eligibleStaff = await _db.Staff
//    //            .Where(s => s.IsActive)
//    //            .Where(s =>
//    //                s.StaffServices
//    //                    .Where(ss => serviceIds.Contains(ss.ServiceId))
//    //                    .Select(ss => ss.ServiceId)
//    //                    .Distinct()
//    //                    .Count() == serviceIds.Count)
//    //            .Select(s => s.StaffId)
//    //            .ToListAsync();

//    //        var slotDictionary = new Dictionary<string, HashSet<int>>();

//    //        foreach (var staffId in eligibleStaff)
//    //        {
//    //            var availability = await _db.StaffAvailabilities
//    //                .FirstOrDefaultAsync(a =>
//    //                    a.StaffId == staffId &&
//    //                    a.DayOfWeek == dayOfWeek &&
//    //                    a.IsActive);

//    //            if (availability == null)
//    //                continue;

//    //            var appointments = await _db.Appointments
//    //                .Where(a =>
//    //                    a.StaffId == staffId &&
//    //                    a.AppointmentDate == date.Date &&
//    //                    a.Status != "Cancelled")
//    //                .Select(a => new BookedSlotDto
//    //                {
//    //                    StartTime = a.StartTime,
//    //                    EndTime = a.EndTime
//    //                })
//    //                .ToListAsync();

//    //            var slots = BuildSlots(
//    //                date.Date,
//    //                availability.StartTime,
//    //                availability.EndTime,
//    //                totalDuration,
//    //                appointments,
//    //                new List<StaffLeave>()
//    //            );

//    //            foreach (var slot in slots)
//    //            {
//    //                var key = $"{slot.StartTime}-{slot.EndTime}";

//    //                if (!slotDictionary.ContainsKey(key))
//    //                    slotDictionary[key] = new HashSet<int>();

//    //                slotDictionary[key].Add(staffId);
//    //            }
//    //        }

//    //        var result = slotDictionary
//    //            .Select(s => new
//    //            {
//    //                startTime = s.Key.Split('-')[0],
//    //                endTime = s.Key.Split('-')[1],
//    //                availableStaffIds = s.Value
//    //            })
//    //            .OrderBy(s => s.startTime)
//    //            .ToList();

//    //        return Ok(result);
//    //    }
//        /* =========================================================
//           SLOT BUILDER
//        ========================================================= */

//        private static List<TimeSlotResponse> BuildSlots(
//            DateTime bookingDate,
//            TimeSpan availableFrom,
//            TimeSpan availableTo,
//            int durationMinutes,
//            List<BookedSlotDto> bookedSlots,
//            List<StaffLeave> leaves)
//        {
//            var slots = new List<TimeSlotResponse>();
//            var slotInterval = TimeSpan.FromMinutes(15);
//            var duration = TimeSpan.FromMinutes(durationMinutes);
//            var nowTime = DateTime.Now.TimeOfDay;
//            var current = availableFrom;

//            while (current + duration <= availableTo)
//            {
//                var proposedEnd = current + duration;

//               if (bookingDate == DateTime.Now.Date && current < nowTime)
//                    {
//                    current += slotInterval;
//                    continue;
//                }

//                bool blockedByLeave = leaves.Any(l =>
//                {
//                    if (l.StartTime == null || l.EndTime == null)
//                        return true;

//                    return current < l.EndTime &&
//                           proposedEnd > l.StartTime;
//                });

//                if (blockedByLeave)
//                {
//                    current += slotInterval;
//                    continue;
//                }

//                bool overlapsAppointment = bookedSlots.Any(b =>
//                    current < b.EndTime &&
//                    proposedEnd > b.StartTime
//                );

//                if (!overlapsAppointment)
//                {
//                    slots.Add(new TimeSlotResponse
//                    {
//                        StartTime = current,
//                        EndTime = proposedEnd
//                    });
//                }

//                current += slotInterval;
//            }

//            return slots;
//        }
//    }
//}