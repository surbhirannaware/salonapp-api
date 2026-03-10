using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/admin/reports")]
[Authorize(Roles = "Admin")]
public class AdminReportsController : ControllerBase
{
    private readonly SalonDbContext _db;

    public AdminReportsController(SalonDbContext db)
    {
        _db = db;
    }

    // 1️⃣ Appointments summary (date range)
    // GET /api/admin/reports/appointments?from=2026-02-01&to=2026-02-28
    [HttpGet("appointments")]
    public async Task<IActionResult> AppointmentsReport(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        if (from > to)
            return BadRequest("Invalid date range");

        var appointments = _db.Appointments
            .Where(a =>
                a.AppointmentDate.Date >= from.Date &&
                a.AppointmentDate.Date <= to.Date);

        var totalAppointments = await appointments.CountAsync();

        var completedAppointments = await appointments
            .CountAsync(a => a.Status == "Completed");

        var cancelledAppointments = await appointments
            .CountAsync(a => a.Status == "Cancelled");

        var revenue = await _db.Payments
            .Where(p =>
                p.PaymentStatus == "Paid" &&
                p.PaidAt.HasValue &&
                p.PaidAt.Value.Date >= from.Date &&
                p.PaidAt.Value.Date <= to.Date)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        return Ok(new
        {
            From = from.Date,
            To = to.Date,
            TotalAppointments = totalAppointments,
            CompletedAppointments = completedAppointments,
            CancelledAppointments = cancelledAppointments,
            Revenue = revenue
        });
    }

    // 2️⃣ Revenue grouped by day (chart-friendly)
    // GET /api/admin/reports/revenue?from=2026-02-01&to=2026-02-28
    [HttpGet("revenue")]
    public async Task<IActionResult> RevenueReport(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        if (from > to)
            return BadRequest("Invalid date range");

        var revenueByDay = await _db.Payments
            .Where(p =>
                p.PaymentStatus == "Paid" &&
                p.PaidAt.HasValue &&
                p.PaidAt.Value.Date >= from.Date &&
                p.PaidAt.Value.Date <= to.Date)
            .GroupBy(p => p.PaidAt.Value.Date)
            .Select(g => new
            {
                Date = g.Key,
                Revenue = g.Sum(p => p.Amount)
            })
            .OrderBy(r => r.Date)
            .ToListAsync();

        return Ok(revenueByDay);
    }
}
