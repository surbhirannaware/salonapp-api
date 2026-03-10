using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonApp.Controllers.DTOs;
using SalonApp.Domain.Entities;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly SalonDbContext _db;

    public AdminDashboardController(SalonDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Summary([FromQuery] DateTime? date)
    {
        var selectedDate = date?.Date ?? DateTime.UtcNow.Date;

        var totalAppointments = await _db.Appointments
            .CountAsync(a => a.AppointmentDate.Date == selectedDate);

        var completedAppointments = await _db.Appointments
            .CountAsync(a =>
                a.AppointmentDate.Date == selectedDate &&
                a.Status == "Completed");

        var cancelledAppointments = await _db.Appointments
            .CountAsync(a =>
                a.AppointmentDate.Date == selectedDate &&
                a.Status == "Cancelled");

        var revenue = await _db.Payments
            .Where(p =>
                p.PaymentStatus == "Paid" &&
                p.PaidAt.HasValue &&
                p.PaidAt.Value.Date == selectedDate)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        return Ok(new
        {
            Date = selectedDate,
            TotalAppointments = totalAppointments,
            CompletedAppointments = completedAppointments,
            CancelledAppointments = cancelledAppointments,
            Revenue = revenue
        });
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> Appointments([FromQuery] DateTime? date)
    {
        var selectedDate = date?.Date ?? DateTime.UtcNow.Date;

        var appointments = await _db.Appointments
            .Where(a => a.AppointmentDate.Date == selectedDate)
            .Include(a => a.Staff)
                .ThenInclude(s => s.User)
            .Include(a => a.AppointmentServices)
                .ThenInclude(x => x.Service)
            .Include(a => a.Payment)
            .OrderBy(a => a.StartTime)
            .Select(a => new
            {
                a.AppointmentId,
                a.StartTime,
                a.EndTime,
                CustomerName = a.CustomerName,
                StaffName = a.Staff.User.FullName,
                Services = a.AppointmentServices
                    .Select(x => x.Service.ServiceName)
                    .ToList(),
                AppointmentStatus = a.Status,
                PaymentStatus = a.Payment != null
                    ? a.Payment.PaymentStatus
                    : "Pending",
                PaymentMethod = a.Payment != null
                    ? a.Payment.PaymentMethod
                    : null,
                TransactionId = a.Payment != null
                    ? a.Payment.TransactionId
                    : null,
                TotalAmount = a.Payment != null
                    ? a.Payment.Amount
                    : a.AppointmentServices.Sum(x => x.PriceAtBooking)
            })
            .ToListAsync();

        return Ok(appointments);
    }

    [HttpGet("weekly")]
    public async Task<IActionResult> WeeklyRevenue()
    {
        var today = DateTime.UtcNow.Date;
        var startDate = today.AddDays(-6);

        var revenueData = await _db.Payments
            .Where(p =>
                p.PaymentStatus == "Paid" &&
                p.PaidAt.HasValue &&
                p.PaidAt.Value.Date >= startDate &&
                p.PaidAt.Value.Date <= today)
            .GroupBy(p => p.PaidAt!.Value.Date)
            .Select(g => new
            {
                Date = g.Key,
                Revenue = g.Sum(x => x.Amount)
            })
            .ToListAsync();

        var result = Enumerable.Range(0, 7)
            .Select(i => startDate.AddDays(i))
            .Select(d => new
            {
                Date = d,
                Revenue = revenueData.FirstOrDefault(x => x.Date == d)?.Revenue ?? 0
            });

        return Ok(result);
    }

    [HttpPut("appointments/{id}")]
    public async Task<IActionResult> UpdateAppointment(
        int id,
        [FromBody] UpdateAdminAppointmentDto dto)
    {
        if (dto == null)
            return BadRequest("Request body is required.");

        var appointment = await _db.Appointments
            .Include(a => a.AppointmentServices)
            .Include(a => a.Payment)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);

        if (appointment == null)
            return NotFound("Appointment not found.");

        var validStatuses = new[] { "Booked", "Completed", "Cancelled" };
        var validPaymentStatuses = new[] { "Pending", "Paid", "Failed" };
        var validPaymentMethods = new[] { "Cash", "UPI", "Card" };

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            if (!validStatuses.Contains(dto.Status))
                return BadRequest("Invalid appointment status.");

            appointment.Status = dto.Status;
        }

        if (!string.IsNullOrWhiteSpace(dto.PaymentStatus))
        {
            if (!validPaymentStatuses.Contains(dto.PaymentStatus))
                return BadRequest("Invalid payment status.");

            var totalAmount = appointment.AppointmentServices.Sum(x => x.PriceAtBooking);

            if (appointment.Payment == null)
            {
                appointment.Payment = new Payment
                {
                    AppointmentId = appointment.AppointmentId,
                    Amount = totalAmount,
                    PaymentStatus = dto.PaymentStatus,
                    PaymentMethod = !string.IsNullOrWhiteSpace(dto.PaymentMethod)
                        ? dto.PaymentMethod
                        : null,
                    TransactionId = dto.TransactionId,
                    PaidAt = dto.PaymentStatus == "Paid" ? DateTime.UtcNow : null
                };

                _db.Payments.Add(appointment.Payment);
            }
            else
            {
                appointment.Payment.Amount = totalAmount;
                appointment.Payment.PaymentStatus = dto.PaymentStatus;
                appointment.Payment.TransactionId = dto.TransactionId;
                appointment.Payment.PaidAt = dto.PaymentStatus == "Paid"
                    ? DateTime.UtcNow
                    : null;
            }

            if (!string.IsNullOrWhiteSpace(dto.PaymentMethod))
            {
                if (!validPaymentMethods.Contains(dto.PaymentMethod))
                    return BadRequest("Invalid payment method.");

                appointment.Payment!.PaymentMethod = dto.PaymentMethod;
            }
        }
        else if (!string.IsNullOrWhiteSpace(dto.PaymentMethod))
        {
            return BadRequest("Payment status is required when payment method is provided.");
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            Message = "Appointment updated successfully.",
            AppointmentId = appointment.AppointmentId,
            Status = appointment.Status,
            PaymentStatus = appointment.Payment?.PaymentStatus ?? "Pending",
            PaymentMethod = appointment.Payment?.PaymentMethod,
            TransactionId = appointment.Payment?.TransactionId,
            TotalAmount = appointment.Payment?.Amount
                ?? appointment.AppointmentServices.Sum(x => x.PriceAtBooking)
        });
    }
}