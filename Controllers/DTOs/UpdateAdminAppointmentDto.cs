namespace SalonApp.Controllers.DTOs
{
    public class UpdateAdminAppointmentDto
    {
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
    }
}