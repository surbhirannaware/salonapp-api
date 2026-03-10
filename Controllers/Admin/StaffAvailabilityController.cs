using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonApp.Controllers.DTOs;
using SalonApp.Domain.Entities;

namespace SalonApp.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/staff/{staffId}/availability")]
    public class StaffAvailabilityController : ControllerBase
    {
        private readonly SalonDbContext _db;

        public StaffAvailabilityController(SalonDbContext db)
        {
            _db = db;
        }

        // 📥 Get weekly availability
        [HttpGet]
        public async Task<IActionResult> Get(int staffId)
        {
            var data = await _db.StaffAvailabilities
                .Where(a => a.StaffId == staffId)
                .OrderBy(a => a.DayOfWeek)
                .ToListAsync();

            return Ok(data);
        }

        // ➕ Add / Update availability
        [HttpPost]
        public async Task<IActionResult> Upsert(
            int staffId,
            [FromBody] StaffAvailabilityDto dto)
        {
            if (dto.StartTime >= dto.EndTime)
                return BadRequest("StartTime must be before EndTime");

            var availability = await _db.StaffAvailabilities
                .FirstOrDefaultAsync(a =>
                    a.StaffId == staffId &&
                    a.DayOfWeek == dto.DayOfWeek);

            if (availability == null)
            {
                availability = new StaffAvailability
                {
                    StaffId = staffId,
                    DayOfWeek = dto.DayOfWeek,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime
                };
                _db.StaffAvailabilities.Add(availability);
            }
            else
            {
                availability.StartTime = dto.StartTime;
                availability.EndTime = dto.EndTime;
                availability.IsActive = true;
            }

            await _db.SaveChangesAsync();
            return Ok("Availability saved");
        }

        // ❌ Disable availability
        [HttpDelete("{dayOfWeek}")]
        public async Task<IActionResult> Disable(int staffId, int dayOfWeek)
        {
            var availability = await _db.StaffAvailabilities
                .FirstOrDefaultAsync(a =>
                    a.StaffId == staffId &&
                    a.DayOfWeek == dayOfWeek);

            if (availability == null)
                return NotFound();

            availability.IsActive = false;
            await _db.SaveChangesAsync();

            return Ok("Availability disabled");
        }
    }

}
