using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/staff/dashboard")]
[Authorize(Roles = "Staff")]
public class StaffDashboardController : ControllerBase
{
    private readonly SalonDbContext _db;

    public StaffDashboardController(SalonDbContext db)
    {
        _db = db;
    }

    // ✅ TODAY DASHBOARD (Summary + List)
    [HttpGet("today")]
    public async Task<IActionResult> GetTodayDashboard()
    {
        var userId = User.GetUserId();
        var today = DateTime.UtcNow.Date;

        var staff = await _db.Staff
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

        if (staff == null)
            return Unauthorized("Staff not found");

        var appointments = await _db.Appointments
            .Where(a => a.StaffId == staff.StaffId &&
                        a.AppointmentDate == today)
            .Include(a => a.CreatedByUser)
            .Include(a => a.AppointmentServices)
                .ThenInclude(s => s.Service)
            .OrderBy(a => a.StartTime)
            .ToListAsync();

        var total = appointments.Count;
        var completed = appointments.Count(a => a.Status == "Completed");
        var pending = appointments.Count(a => a.Status == "Booked");

        var earnings = appointments
            .Where(a => a.Status == "Completed")
            .SelectMany(a => a.AppointmentServices)
            .Sum(s => s.Service.Price);

        return Ok(new
        {
            totalAppointments = total,
            completedAppointments = completed,
            pendingAppointments = pending,
            todayEarnings = earnings,
            appointments = appointments.Select(a => new
            {
                appointmentId = a.AppointmentId,
                customerName = a.CreatedByUser.FullName,
                startTime = a.StartTime.ToString(@"hh\:mm"),
                endTime = a.EndTime.ToString(@"hh\:mm"),
                status = a.Status,
                services = a.AppointmentServices
                    .Select(s => s.Service.ServiceName)
                    .ToList()
            })
        });
    }

    // ✅ COMPLETE APPOINTMENT
    [HttpPut("appointments/{id}/complete")]
    public async Task<IActionResult> CompleteAppointment(int id)
    {
        var userId = User.GetUserId();
        var today = DateTime.UtcNow.Date;

        var staff = await _db.Staff
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

        if (staff == null)
        {
            return BadRequest("Staff record not found");
        }

        var appointment = await _db.Appointments
            .FirstOrDefaultAsync(a =>
                a.AppointmentId == id &&
                a.StaffId == staff.StaffId);

        if (appointment == null)
            return NotFound("Appointment not found");

        if (appointment.AppointmentDate != today)
            return BadRequest("Only today's appointments can be completed");

        if (appointment.Status != "Booked")
            return BadRequest("Only booked appointments can be completed");

        appointment.Status = "Completed";
        await _db.SaveChangesAsync();

        return Ok(new { message = "Appointment completed" });
    }

    // ✅ UPCOMING APPOINTMENTS
    [HttpGet("appointments/upcoming")]
    public async Task<IActionResult> GetUpcoming()
    {
        var userId = User.GetUserId();
        var today = DateTime.UtcNow.Date;

        var staff = await _db.Staff
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

        if (staff == null)
            return Unauthorized("Staff not found");

        var appointments = await _db.Appointments
            .Where(a =>
                a.StaffId == staff.StaffId &&
                a.AppointmentDate >= today &&
                a.Status == "Booked")
            .Include(a => a.CreatedByUser)
            .Include(a => a.AppointmentServices)
                .ThenInclude(s => s.Service)
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .ToListAsync();

        return Ok(appointments.Select(a => new
        {
            appointmentId = a.AppointmentId,
            appointmentDate = a.AppointmentDate,
            startTime = a.StartTime.ToString(@"hh\:mm"),
            endTime = a.EndTime.ToString(@"hh\:mm"),
            customerName = a.CreatedByUser.FullName,
            services = a.AppointmentServices
                .Select(s => s.Service.ServiceName),
            status = a.Status,
            isUpcoming = a.AppointmentDate > today,
            isOngoing = a.AppointmentDate == today
        }));
    }
}