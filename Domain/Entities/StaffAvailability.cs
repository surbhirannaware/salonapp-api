namespace SalonApp.Domain.Entities
{
    public class StaffAvailability
    {
        public int AvailabilityId { get; set; }
        public int StaffId { get; set; }
        public int DayOfWeek { get; set; } // 0-6
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; } = true;

        public Staff Staff { get; set; } = null!;
    }

}
