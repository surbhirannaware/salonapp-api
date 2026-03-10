namespace SalonApp.Controllers.DTOs
{
    public class StaffTodayAppointment
    {
        public int AppointmentId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string CustomerName { get; set; } = null!;
        public List<string> Services { get; set; } = new();
        public string Status { get; set; } = null!;
    }

}
