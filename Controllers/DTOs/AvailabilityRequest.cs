namespace SalonApp.Controllers.DTOs
{
    public class AvailabilityRequest
    {
        public DateTime Date { get; set; }
        public List<int> ServiceIds { get; set; } = new();
    }
}
