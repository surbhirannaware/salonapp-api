namespace SalonApp.Controllers.DTOs
{
    public class CustomerAppointmentDto
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public List<string> Services { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = "Pending";
        public decimal TotalAmount { get; set; }
        public string? Description { get; set; }
    }
}