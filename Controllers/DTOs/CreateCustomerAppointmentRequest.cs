namespace SalonApp.Controllers.DTOs
{
    public class CreateCustomerAppointmentRequest
    {
        public List<int> ServiceIds { get; set; } = new();
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public string? Description { get; set; }
    }
}
