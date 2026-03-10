using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonApp.Controllers.DTOs;
using SalonApp.Domain.Entities;

namespace SalonApp.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly SalonDbContext _db;

        public PaymentsController(SalonDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Pay(CreatePaymentRequest request)
        {
            var appointment = await _db.Appointments
                .Include(a => a.AppointmentServices)
                .FirstOrDefaultAsync(a => a.AppointmentId == request.AppointmentId);

            if (appointment == null)
                return BadRequest("Invalid appointment");

            if (appointment.Status == "Cancelled")
                return BadRequest("Cancelled appointment");

            bool alreadyPaid = await _db.Payments
                .AnyAsync(p => p.AppointmentId == request.AppointmentId);

            if (alreadyPaid)
                return BadRequest("Payment already done");

            var amount = appointment.AppointmentServices
                .Sum(s => s.PriceAtBooking);


            var payment = new Payment
            {
                AppointmentId = request.AppointmentId,
                //Amount = request.Amount,
                PaymentMethod = request.PaymentMethod,
                TransactionId = request.TransactionId,
                PaymentStatus = "Paid",
                PaidAt = DateTime.UtcNow
            };

            _db.Payments.Add(payment);

            // Mark appointment completed
            appointment.Status = "Completed";

            await _db.SaveChangesAsync();

            return Ok(new
            {
                payment.PaymentId,
                payment.PaymentMethod,
                payment.Amount,
                payment.PaidAt
            });
        }


        [HttpGet("appointment/{appointmentId}")]
        public async Task<IActionResult> GetInvoice(int appointmentId)
        {
            var appointment = await _db.Appointments
                .Include(a => a.CreatedByUser)
                .Include(a => a.Staff)
                    .ThenInclude(s => s.User)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(s => s.Service)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
                return NotFound("Appointment not found");

            var payment = await _db.Payments
                .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);

            if (payment == null)
                return BadRequest("Payment not completed");

            var response = new InvoiceResponse
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,

                CustomerName = appointment.CreatedByUser.FullName,
                StaffName = appointment.Staff.User.FullName,

                Services = appointment.AppointmentServices.Select(s => new InvoiceServiceDto
                {
                    ServiceName = s.Service.ServiceName,
                    Price = s.PriceAtBooking,
                    DurationMinutes = s.DurationMinutes
                }).ToList(),

                TotalAmount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                PaidAt = (DateTime)payment.PaidAt
            };

            return Ok(response);
        }

       
        
        
    }


}
