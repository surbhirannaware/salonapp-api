using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonApp.Controllers.DTOs;
using SalonApp.Domain.Entities;

namespace SalonApp.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/staff/{staffId}/leaves")]
    public class StaffLeaveController : ControllerBase
    {
        private readonly SalonDbContext _db;

        public StaffLeaveController(SalonDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> AddLeave(
            int staffId,
            [FromBody] CreateStaffLeaveDto dto)
        {
            var leave = new StaffLeave
            {
                StaffId = staffId,
                LeaveDate = dto.LeaveDate.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Reason = dto.Reason
            };

            _db.StaffLeaves.Add(leave);
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetLeaves(int staffId)
        {
            var leaves = await _db.StaffLeaves
                .Where(l => l.StaffId == staffId)
                .OrderBy(l => l.LeaveDate)
                .ToListAsync();

            return Ok(leaves);
        }
    }

}
