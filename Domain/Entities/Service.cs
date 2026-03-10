namespace SalonApp.Domain.Entities
{
    public class Service
    {
        public int ServiceId { get; set; }
        public int CategoryId { get; set; }
        public string ServiceName { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsActive { get; set; }

        public ServiceCategory Category { get; set; } = null!;
        public ICollection<StaffService> StaffServices { get; set; } = new List<StaffService>();
        public ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();
    }

}

