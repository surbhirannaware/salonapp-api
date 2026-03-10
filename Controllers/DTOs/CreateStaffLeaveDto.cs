namespace SalonApp.Controllers.DTOs
{
    public class CreateStaffLeaveDto
    {
        public DateTime LeaveDate { get; set; }

        // null = full day leave
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public string? Reason { get; set; }
    }

}
