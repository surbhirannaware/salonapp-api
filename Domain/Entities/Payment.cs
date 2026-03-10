namespace SalonApp.Domain.Entities
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int AppointmentId { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? PaidAt { get; set; }

        public Appointment Appointment { get; set; } = null!;
    }
}
