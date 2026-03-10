namespace SalonApp.Controllers.DTOs
{
    public class CreateAppointmentRequest
    {
        public List<int> ServiceIds { get; set; } = new();

        public DateTime AppointmentDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public Guid? CustomerId { get; set; }

        public string? CustomerName { get; set; }

        public bool IsNewCustomer { get; set; }

        public string? Description { get; set; }
    }
}




