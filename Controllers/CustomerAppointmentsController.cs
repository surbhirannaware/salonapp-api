using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonApp.Controllers.DTOs;
using SalonApp.Domain.Entities;

[ApiController]
[Route("api/customer/appointments")]
[Authorize(Roles = "Customer")]
public class CustomerAppointmentsController : ControllerBase
{
    private readonly SalonDbContext _db;

    public CustomerAppointmentsController(SalonDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAppointment(CreateCustomerAppointmentRequest request)
    {
        var userId = User.GetUserId();

        var customer = await _db.Users
            .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);

        if (customer == null)
            return Unauthorized("Customer not found");

        if (request.ServiceIds == null || !request.ServiceIds.Any())
            return BadRequest("Please select at least one service.");

        var services = await _db.Services
            .Where(s => request.ServiceIds.Contains(s.ServiceId) && s.IsActive)
            .ToListAsync();

        if (services.Count != request.ServiceIds.Count)
            return BadRequest("One or more selected services are invalid.");

        var totalDuration = services.Sum(s => s.DurationMinutes);
        var endTime = request.StartTime.Add(TimeSpan.FromMinutes(totalDuration));

        var appointmentDate = request.AppointmentDate.Date;

        // Example: auto-assign first available staff
        var availableStaff = await _db.Staff
            .Where(s => s.IsActive)
            .ToListAsync();

        int? assignedStaffId = null;

        foreach (var staff in availableStaff)
        {
            var hasConflict = await _db.Appointments.AnyAsync(a =>
                a.StaffId == staff.StaffId &&
                a.AppointmentDate == appointmentDate &&
                a.Status != "Cancelled" &&
                request.StartTime < a.EndTime &&
                endTime > a.StartTime);

            if (!hasConflict)
            {
                assignedStaffId = staff.StaffId;
                break;
            }
        }

        if (assignedStaffId == null)
            return BadRequest("No staff available for the selected slot.");

        var appointment = new Appointment
        {
            CreatedByUserId = userId,
            CustomerUserId = userId,
            CustomerName = customer.FullName,
            Description = request.Description,
            StaffId = assignedStaffId.Value,
            AppointmentDate = appointmentDate,
            StartTime = request.StartTime,
            EndTime = endTime,
            Status = "Booked",
            CreatedAt = DateTime.UtcNow
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        foreach (var service in services)
        {
            _db.AppointmentServices.Add(new AppointmentService
            {
                AppointmentId = appointment.AppointmentId,
                ServiceId = service.ServiceId,
                PriceAtBooking = service.Price,
                DurationMinutes = service.DurationMinutes
            });
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Appointment booked successfully",
            appointmentId = appointment.AppointmentId
        });
    }
}