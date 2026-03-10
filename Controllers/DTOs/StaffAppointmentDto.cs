namespace SalonApp.Controllers.DTOs
{
    public class StaffAppointmentDto
    {
        public int AppointmentId { get; set; }
        public string CustomerName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Status { get; set; }
        public List<string> Services { get; set; }
    }
}
