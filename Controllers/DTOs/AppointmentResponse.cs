namespace SalonApp.Controllers.DTOs
{
    public class AppointmentResponse
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } = null!;
        public decimal TotalAmount { get; set; }
    }

}
