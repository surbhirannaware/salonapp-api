using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SalonApp.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly SalonDbContext _db;

        public ReportsController(SalonDbContext db)
        {
            _db = db;
        }

        [HttpGet("staff-earnings")]
        public async Task<IActionResult> GetStaffEarnings(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            var report = await _db.Payments
                .Where(p =>
                    p.PaymentStatus == "Paid" &&
                    p.PaidAt >= from.Date &&
                    p.PaidAt <= to.Date.AddDays(1))
                .Join(
                    _db.Appointments,
                    p => p.AppointmentId,
                    a => a.AppointmentId,
                    (p, a) => new { p, a }
                )
                .Join(
                    _db.Staff,
                    pa => pa.a.StaffId,
                    s => s.StaffId,
                    (pa, s) => new
                    {
                        StaffId = s.StaffId,
                        StaffName = s.User.FullName,
                        Amount = pa.p.Amount
                    }
                )
                .GroupBy(x => new { x.StaffId, x.StaffName })
                .Select(g => new
                {
                    g.Key.StaffId,
                    g.Key.StaffName,
                    TotalEarnings = g.Sum(x => x.Amount)
                })
                .OrderByDescending(x => x.TotalEarnings)
                .ToListAsync();

            return Ok(report);
        }


        [HttpGet("staff-earnings/today")]
        public async Task<IActionResult> GetTodayStaffEarnings()
        {
            var today = DateTime.UtcNow.Date;

            var report = await _db.Payments
                .Where(p =>
                    p.PaymentStatus == "Paid" &&
                    p.PaidAt >= today &&
                    p.PaidAt < today.AddDays(1))
                .Join(
                    _db.Appointments,
                    p => p.AppointmentId,
                    a => a.AppointmentId,
                    (p, a) => new { p, a }
                )
                .Join(
                    _db.Staff,
                    pa => pa.a.StaffId,
                    s => s.StaffId,
                    (pa, s) => new
                    {
                        StaffName = s.User.FullName,
                        Amount = pa.p.Amount
                    }
                )
                .GroupBy(x => x.StaffName)
                .Select(g => new
                {
                    StaffName = g.Key,
                    TotalEarnings = g.Sum(x => x.Amount)
                })
                .ToListAsync();

            return Ok(report);
        }

    }
}
