using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonApp.Controllers.DTOs;
using System.Security.Claims;

namespace SalonApp.Controllers
{
    [ApiController]
    [Route("api/customer/dashboard")]
    [Authorize(Roles = "Customer")]
    public class CustomerDashboardController : ControllerBase
    {
        private readonly SalonDbContext _db;

        public CustomerDashboardController(SalonDbContext db)
        {
            _db = db;
        }

        private Guid GetUserId()
        {
            var claim =
                User.FindFirst(ClaimTypes.NameIdentifier) ??
                User.FindFirst("sub");

            if (claim == null)
                throw new UnauthorizedAccessException("User ID not found in token.");

            return Guid.Parse(claim.Value);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var userId = GetUserId();
            var today = DateTime.UtcNow.Date;

            var appointments = _db.Appointments
                .Include(a => a.Payment)
                .Where(a => a.CustomerUserId == userId);

            var upcomingCount = await appointments
                .CountAsync(a => a.AppointmentDate.Date >= today && a.Status == "Booked");

            var completedCount = await appointments
                .CountAsync(a => a.Status == "Completed");

            var cancelledCount = await appointments
                .CountAsync(a => a.Status == "Cancelled");

            var totalSpent = await appointments
                .Where(a => a.Payment != null && a.Payment.PaymentStatus == "Paid")
                .SumAsync(a => (decimal?)a.Payment!.Amount) ?? 0;

            return Ok(new
            {
                UpcomingCount = upcomingCount,
                CompletedCount = completedCount,
                CancelledCount = cancelledCount,
                TotalSpent = totalSpent
            });
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingAppointments()
        {
            var userId = GetUserId();
            var today = DateTime.UtcNow.Date;

            var appointments = await _db.Appointments
                .Where(a =>
                    a.CustomerUserId == userId &&
                    a.AppointmentDate.Date >= today &&
                    a.Status == "Booked")
                .Include(a => a.Staff)
                    .ThenInclude(s => s.User)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(x => x.Service)
                .Include(a => a.Payment)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Select(a => new CustomerAppointmentDto
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    StaffName = a.Staff.User.FullName,
                    Services = a.AppointmentServices
                        .Select(x => x.Service.ServiceName)
                        .ToList(),
                    Status = a.Status,
                    PaymentStatus = a.Payment != null
                        ? a.Payment.PaymentStatus ?? "Pending"
                        : "Pending",
                    TotalAmount = a.Payment != null
                        ? a.Payment.Amount
                        : a.AppointmentServices.Sum(x => x.PriceAtBooking),
                    Description = a.Description
                })
                .ToListAsync();

            return Ok(appointments);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userId = GetUserId();
            var today = DateTime.UtcNow.Date;

            var appointments = await _db.Appointments
                .Where(a =>
                    a.CustomerUserId == userId &&
                    (a.AppointmentDate.Date < today || a.Status != "Booked"))
                .Include(a => a.Staff)
                    .ThenInclude(s => s.User)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(x => x.Service)
                .Include(a => a.Payment)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.StartTime)
                .Select(a => new CustomerAppointmentDto
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    StaffName = a.Staff.User.FullName,
                    Services = a.AppointmentServices
                        .Select(x => x.Service.ServiceName)
                        .ToList(),
                    Status = a.Status,
                    PaymentStatus = a.Payment != null
                        ? a.Payment.PaymentStatus ?? "Pending"
                        : "Pending",
                    TotalAmount = a.Payment != null
                        ? a.Payment.Amount
                        : a.AppointmentServices.Sum(x => x.PriceAtBooking),
                    Description = a.Description
                })
                .ToListAsync();

            return Ok(appointments);
        }
    }
}