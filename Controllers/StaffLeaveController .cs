using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonApp.Controllers.DTOs;
using SalonApp.Domain.Entities;


namespace SalonApp.Controllers
{
    [ApiController]
    [Route("api/staff-leaves")]
    public class StaffLeaveController : ControllerBase
    {
        private readonly SalonDbContext _db;

        public StaffLeaveController(SalonDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> AddLeave(AddLeaveDto dto)
        {
            var userId = User.GetUserId();

            var staff = await _db.Staff
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

            if (staff == null)
                return Unauthorized();

            if ((dto.StartTime == null && dto.EndTime != null) ||
                (dto.StartTime != null && dto.EndTime == null))
                return BadRequest("Invalid leave time");

            var alreadyExists = await _db.StaffLeaves
    .AnyAsync(l =>
        l.StaffId == staff.StaffId &&
        l.LeaveDate.Date == dto.LeaveDate.Date &&
        l.IsActive);

            if (alreadyExists)
                return BadRequest("Leave already applied for this date.");

            var leave = new StaffLeave
            {
                StaffId = staff.StaffId,
                LeaveDate = dto.LeaveDate.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Reason = dto.Reason,
                Status = "Pending",
                IsActive = true
            };

            _db.StaffLeaves.Add(leave);
            await _db.SaveChangesAsync();

            return Ok(leave);
        }


    

        [HttpGet("my-leaves")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> GetMyLeaves()
        {
            var userId = User.GetUserId();

            var staff = await _db.Staff
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);
                

            if (staff == null)
                return Unauthorized();

            var leaves = await _db.StaffLeaves
                .Where(l => l.StaffId == staff.StaffId && l.IsActive)
                .OrderByDescending(l => l.LeaveDate)
                .ToListAsync();

            return Ok(leaves);
        }

        [HttpDelete("{leaveId}")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> CancelLeave(int leaveId)
        {
            var userId = User.GetUserId();

            var staff = await _db.Staff
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

            if (staff == null)
                return Unauthorized();

            var leave = await _db.StaffLeaves
                .FirstOrDefaultAsync(l =>
                    l.StaffLeaveId == leaveId &&
                    l.StaffId == staff.StaffId);

            if (leave == null)
                return NotFound();

            if (leave.Status == "Approved")
                return BadRequest("Approved leave cannot be cancelled.");

            leave.IsActive = false;

            await _db.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/pending")]
        public async Task<IActionResult> GetPendingLeaves()
        {
            var leaves = await _db.StaffLeaves
                .Include(l => l.Staff)
                .ThenInclude(s => s.User)
                .Where(l => l.Status == "Pending" && l.IsActive)
                .ToListAsync();

            return Ok(leaves);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("admin/{id}")]
        public async Task<IActionResult> UpdateLeaveStatus(int id, string status, string? remark)
        {
            var leave = await _db.StaffLeaves.FindAsync(id);

            if (leave == null)
                return NotFound();

            leave.Status = status;
            leave.AdminRemark = remark;

            await _db.SaveChangesAsync();

            return Ok(leave);
        }
    }
}