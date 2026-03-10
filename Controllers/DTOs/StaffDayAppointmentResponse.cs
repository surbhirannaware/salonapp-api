namespace SalonApp.Controllers.DTOs
{
    public class StaffDayAppointmentResponse
    {
        public int AppointmentId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string ClientName { get; set; } = null!;
        public string Status { get; set; } = null!;
    }

}
