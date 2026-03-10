namespace SalonApp.Domain.Entities
{
    public class User
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Email { get; set; } 
        public string PhoneNumber { get; set; }
        public string PasswordHash { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public ICollection<Appointment> CreatedAppointments { get; set; } = new List<Appointment>();
        public ICollection<Appointment> CustomerAppointments { get; set; } = new List<Appointment>();
    }
}