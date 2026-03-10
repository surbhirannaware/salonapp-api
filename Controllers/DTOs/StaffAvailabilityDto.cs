namespace SalonApp.Controllers.DTOs
{
    public class StaffAvailabilityDto
    {
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

}
