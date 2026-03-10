namespace SalonApp.Domain.Entities
{
    public class UserRole
    {
        public int UserRoleId { get; set; }
        public Guid UserId { get; set; }
        public int RoleId { get; set; }

        public User User { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }
}
