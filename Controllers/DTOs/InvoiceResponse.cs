namespace SalonApp.Controllers.DTOs
{
    public class InvoiceResponse
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string StaffName { get; set; } = null!;
        public string CustomerName { get; set; } = null!;

        public List<InvoiceServiceDto> Services { get; set; } = new();

        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public DateTime PaidAt { get; set; }
    }

}
