namespace SalonApp.Domain.Entities
{
    public class StaffService
    {
        public int StaffServiceId { get; set; }
        public int StaffId { get; set; }
        public int ServiceId { get; set; }

        public Staff Staff { get; set; } = null!;
        public Service Service { get; set; } = null!;
    }
}
