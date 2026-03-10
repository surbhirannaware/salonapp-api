using SalonApp.Domain.Entities;

public class Staff
{
    public int StaffId { get; set; }

    public Guid UserId { get; set; }

    public string? Specialization { get; set; }
    public bool IsActive { get; set; }

    public User User { get; set; } = null!;

    public ICollection<Appointment> Appointments { get; set; }
        = new List<Appointment>();

    // ✅ ADD THIS
    public ICollection<StaffService> StaffServices { get; set; }
        = new List<StaffService>();
}
