namespace SalonApp.Domain.Entities
{
    public class StaffLeave
    {   public int StaffLeaveId { get; set; }
        public int StaffId { get; set; }

        // Date-specific leave
        public DateTime LeaveDate { get; set; }
        // Optional partial leave
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Status { get; set; } = "Pending";
        // Pending | Approved | Rejected

        public string? AdminRemark { get; set; }
        public string Reason { get; set; } = null!;
        public bool IsActive { get; set; } = true;

        public Staff Staff { get; set; } = null!;
    }
}
