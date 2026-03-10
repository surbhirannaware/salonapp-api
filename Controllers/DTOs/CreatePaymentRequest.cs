namespace SalonApp.Controllers.DTOs
{
    public class CreatePaymentRequest
    {
        public int AppointmentId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } // Cash / UPI
        public string? TransactionId { get; set; }
    }
}
