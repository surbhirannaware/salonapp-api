using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalonApp.Controllers.DTOs;
using SalonApp.Domain.Entities;

[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly SalonDbContext _db;

    public AppointmentsController(SalonDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request)
    {
        var userId = User.GetUserId();
        var isAdmin = User.IsInRole("Admin");
        var isCustomer = User.IsInRole("Customer");

        if (request.ServiceIds == null || !request.ServiceIds.Any())
            return BadRequest("At least one service is required");

        Guid? customerUserId = null;
        string customerName = "";

        if (isCustomer)
        {
            var loggedInCustomer = await (
                from u in _db.Users
                join ur in _db.UserRoles on u.UserId equals ur.UserId
                join r in _db.Roles on ur.RoleId equals r.RoleId
                where r.RoleName == "Customer" && u.UserId == userId
                select new
                {
                    u.UserId,
                    u.FullName
                }
            ).FirstOrDefaultAsync();

            if (loggedInCustomer == null)
                return BadRequest("Logged in customer not found");

            customerUserId = loggedInCustomer.UserId;
            customerName = loggedInCustomer.FullName?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(customerName))
                return BadRequest("Customer name not found");
        }
        else if (isAdmin)
        {
            customerName = request.CustomerName?.Trim() ?? "";

            if (request.IsNewCustomer)
            {
                if (string.IsNullOrWhiteSpace(customerName))
                    return BadRequest("Customer name is required for new customer");
            }
            else
            {
                if (!request.CustomerId.HasValue)
                    return BadRequest("Please select an existing customer");

                var existingCustomer = await (
                    from u in _db.Users
                    join ur in _db.UserRoles on u.UserId equals ur.UserId
                    join r in _db.Roles on ur.RoleId equals r.RoleId
                    where r.RoleName == "Customer" && u.UserId == request.CustomerId.Value
                    select new
                    {
                        u.UserId,
                        u.FullName
                    }
                ).FirstOrDefaultAsync();

                if (existingCustomer == null)
                    return BadRequest("Selected customer not found");

                customerUserId = existingCustomer.UserId;
                customerName = existingCustomer.FullName?.Trim() ?? "";
            }
        }
        else
        {
            return Forbid();
        }

        var services = await _db.Services
            .Where(s => request.ServiceIds.Contains(s.ServiceId) && s.IsActive)
            .ToListAsync();

        if (!services.Any())
            return BadRequest("Invalid services");

        if (services.Count != request.ServiceIds.Distinct().Count())
            return BadRequest("One or more selected services are invalid");

        int duration = services.Sum(s => s.DurationMinutes);
        var appointmentDate = request.AppointmentDate.Date;
        var appointmentEnd = request.StartTime.Add(TimeSpan.FromMinutes(duration));

        var staff = await _db.Staff
            .Where(s => s.IsActive)
            .Include(s => s.StaffServices)
            .ToListAsync();

        var eligibleStaff = staff
            .Where(s => request.ServiceIds.All(id =>
                s.StaffServices.Any(ss => ss.ServiceId == id)))
            .Select(s => s.StaffId)
            .ToList();

        if (!eligibleStaff.Any())
            return BadRequest("No staff provides selected services");

        var busyStaff = await _db.Appointments
            .Where(a =>
                a.AppointmentDate.Date == appointmentDate &&
                a.Status != "Cancelled" &&
                request.StartTime < a.EndTime &&
                appointmentEnd > a.StartTime)
            .Select(a => a.StaffId)
            .ToListAsync();

        var availableStaff = eligibleStaff
            .FirstOrDefault(id => !busyStaff.Contains(id));

        if (availableStaff == 0)
            return BadRequest("No staff available for this slot");

        var appointment = new Appointment
        {
            AppointmentDate = appointmentDate,
            StartTime = request.StartTime,
            EndTime = appointmentEnd,
            CustomerUserId = customerUserId,
            CustomerName = customerName,
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            Status = "Booked",
            StaffId = availableStaff,
            CreatedByUserId = userId
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
            appointmentId = appointment.AppointmentId,
            appointmentDate = appointment.AppointmentDate,
            startTime = appointment.StartTime,
            endTime = appointment.EndTime,
            customerName = appointment.CustomerName,
            customerUserId = appointment.CustomerUserId,
            status = appointment.Status,
            staffId = appointment.StaffId
        });
    }

    [HttpPut("{appointmentId}/cancel")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> Cancel(int appointmentId)
    {
        var userId = User.GetUserId();
        var isAdmin = User.IsInRole("Admin");
        var isCustomer = User.IsInRole("Customer");

        var appointment = await _db.Appointments
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

        if (appointment == null)
            return NotFound("Appointment not found");

        if (isAdmin && appointment.CreatedByUserId != userId)
            return BadRequest("You can cancel only appointments created by you");

        if (isCustomer && appointment.CustomerUserId != userId)
            return BadRequest("You can cancel only your own appointments");

        if (appointment.Status != "Booked")
            return BadRequest("Only booked appointments can be cancelled");

        var appointmentDateTime = appointment.AppointmentDate.Date.Add(appointment.StartTime);

        if (appointmentDateTime <= DateTime.UtcNow)
            return BadRequest("Past appointments cannot be cancelled");

        appointment.Status = "Cancelled";
        await _db.SaveChangesAsync();

        return Ok(new
        {
            appointment.AppointmentId,
            appointment.Status
        });
    }

    [HttpPut("{appointmentId}/reschedule")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> Reschedule(int appointmentId, RescheduleAppointmentRequest request)
    {
        var userId = User.GetUserId();
        var isAdmin = User.IsInRole("Admin");
        var isCustomer = User.IsInRole("Customer");

        using var tx = await _db.Database
            .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        try
        {
            var appointment = await _db.Appointments
                .Include(a => a.AppointmentServices)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
                return NotFound("Appointment not found");

            if (isAdmin && appointment.CreatedByUserId != userId)
                return BadRequest("You can reschedule only appointments created by you");

            if (isCustomer && appointment.CustomerUserId != userId)
                return BadRequest("You can reschedule only your own appointments");

            if (appointment.Status != "Booked")
                return BadRequest("Only booked appointments can be rescheduled");

            var totalDuration = appointment.AppointmentServices.Sum(s => s.DurationMinutes);
            var newDate = request.AppointmentDate.Date;
            var newEndTime = request.NewStartTime.Add(TimeSpan.FromMinutes(totalDuration));

            bool conflict = await _db.Appointments
                .FromSqlRaw(@"
                SELECT *
                FROM Appointments WITH (UPDLOCK, HOLDLOCK)
                WHERE StaffId = @staffId
                  AND AppointmentDate = @date
                  AND Status <> 'Cancelled'
                  AND AppointmentId <> @appointmentId
                  AND @startTime < EndTime
                  AND @endTime > StartTime",
                    new SqlParameter("@staffId", appointment.StaffId),
                    new SqlParameter("@date", newDate),
                    new SqlParameter("@appointmentId", appointmentId),
                    new SqlParameter("@startTime", request.NewStartTime),
                    new SqlParameter("@endTime", newEndTime)
                )
                .AnyAsync();

            if (conflict)
                return BadRequest("Selected time slot is not available");

            appointment.AppointmentDate = newDate;
            appointment.StartTime = request.NewStartTime;
            appointment.EndTime = newEndTime;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new
            {
                appointment.AppointmentId,
                appointment.AppointmentDate,
                appointment.StartTime,
                appointment.EndTime,
                appointment.Status
            });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    [HttpGet("my")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> MyAppointments([FromQuery] string type = "upcoming")
    {
        var userId = User.GetUserId();
        var today = DateTime.UtcNow.Date;
        var isCustomer = User.IsInRole("Customer");

        var query = _db.Appointments
            .Include(a => a.Staff)
                .ThenInclude(s => s.User)
            .Include(a => a.AppointmentServices)
            .AsQueryable();

        query = isCustomer
            ? query.Where(a => a.CustomerUserId == userId)
            : query.Where(a => a.CreatedByUserId == userId);

        query = type switch
        {
            "past" => query.Where(a =>
                a.AppointmentDate < today ||
                a.Status == "Cancelled" ||
                a.Status == "Completed"),

            "all" => query,

            _ => query.Where(a =>
                a.AppointmentDate >= today &&
                a.Status == "Booked")
        };

        var result = await query
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .Select(a => new MyAppointmentResponse
            {
                AppointmentId = a.AppointmentId,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                StaffName = a.Staff.User.FullName,
                TotalAmount = a.AppointmentServices.Sum(s => s.PriceAtBooking)
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("staff/{staffId}/day")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> StaffDaySchedule(int staffId, [FromQuery] DateTime date)
    {
        var appointments = await _db.Appointments
            .Include(a => a.CreatedByUser)
            .Where(a =>
                a.StaffId == staffId &&
                a.AppointmentDate.Date == date.Date &&
                a.Status != "Cancelled")
            .OrderBy(a => a.StartTime)
            .Select(a => new StaffDayAppointmentResponse
            {
                AppointmentId = a.AppointmentId,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                ClientName = a.CustomerName,
                Status = a.Status
            })
            .ToListAsync();

        return Ok(appointments);
    }

    [HttpGet("today")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetTodayAppointments()
    {
        var today = DateTime.UtcNow.Date;

        var appointments = await _db.Appointments
            .Include(a => a.Staff)
                .ThenInclude(s => s.User)
            .Include(a => a.AppointmentServices)
                .ThenInclude(s => s.Service)
            .Include(a => a.Payment)
            .Where(a => a.AppointmentDate.Date == today)
            .OrderBy(a => a.StartTime)
            .Select(a => new
            {
                a.AppointmentId,
                a.StartTime,
                a.EndTime,
                CustomerName = a.CustomerName,
                StaffName = a.Staff.User.FullName,
                Services = a.AppointmentServices
                    .Select(s => s.Service.ServiceName)
                    .ToList(),
                TotalAmount = a.Payment != null
                    ? a.Payment.Amount
                    : a.AppointmentServices.Sum(s => s.PriceAtBooking),
                PaymentStatus = a.Payment != null
                    ? a.Payment.PaymentStatus
                    : "Pending",
                AppointmentStatus = a.Status
            })
            .ToListAsync();

        return Ok(appointments);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAppointmentStatus(int id, [FromBody] string status)
    {
        var appointment = await _db.Appointments.FindAsync(id);

        if (appointment == null)
            return NotFound();

        appointment.Status = status;
        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpPut("{id}/payment")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePaymentStatus(int id)
    {
        var appointment = await _db.Appointments
            .Include(a => a.Payment)
            .Include(a => a.AppointmentServices)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);

        if (appointment == null)
            return NotFound();

        if (appointment.Payment == null)
        {
            appointment.Payment = new Payment
            {
                AppointmentId = id,
                Amount = appointment.AppointmentServices.Sum(s => s.PriceAtBooking),
                PaymentStatus = "Paid",
                PaidAt = DateTime.UtcNow
            };
        }
        else
        {
            appointment.Payment.PaymentStatus = "Paid";
            appointment.Payment.PaidAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return Ok();
    }
}