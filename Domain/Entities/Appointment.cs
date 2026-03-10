using SalonApp.Domain.Entities;


public class Appointment
{
    public int AppointmentId { get; set; }

    // Who created booking (admin/staff)
    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    // Customer
    public Guid? CustomerUserId { get; set; }
    public User? CustomerUser { get; set; }

    public string CustomerName { get; set; } = null!;
    public string? Description { get; set; }

    public int StaffId { get; set; }

    public DateTime AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public string Status { get; set; } = "Booked";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Staff Staff { get; set; } = null!;
    public Payment? Payment { get; set; }

    public ICollection<AppointmentService> AppointmentServices { get; set; }
        = new List<AppointmentService>();
}

