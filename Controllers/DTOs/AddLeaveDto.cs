namespace SalonApp.Controllers.DTOs
{
    public class AddLeaveDto
    {
        public DateTime LeaveDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Reason { get; set; }
    }
}
