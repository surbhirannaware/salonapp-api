namespace SalonApp.Controllers.DTOs
{
    public class StaffAppointment
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string CustomerName { get; set; } = null!;
        public List<string> Services { get; set; } = new();

        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public string Status { get; set; } = null!;
    }

}
