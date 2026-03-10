namespace SalonApp.Domain.Entities
{
    public class AppointmentService
    {
        public int AppointmentServiceId { get; set; }
        public int AppointmentId { get; set; }
        public int ServiceId { get; set; }
        public decimal PriceAtBooking { get; set; }
        public int DurationMinutes { get; set; }

        public Appointment Appointment { get; set; } = null!;
        public Service Service { get; set; } = null!;
    }
}
