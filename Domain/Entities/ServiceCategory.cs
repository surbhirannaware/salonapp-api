namespace SalonApp.Domain.Entities
{
    public class ServiceCategory
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public bool IsActive { get; set; }

        public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
