namespace SalonApp.Controllers.DTOs
{
    public class AppointmentResponseDto
    {
        public int AppointmentId { get; set; }

        public DateTime AppointmentDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string CustomerName { get; set; }

        public string Status { get; set; }

        public int StaffId { get; set; }
    }
}
