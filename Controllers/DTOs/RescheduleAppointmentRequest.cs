namespace SalonApp.Controllers.DTOs
{
    public class RescheduleAppointmentRequest
    {
        public DateTime AppointmentDate { get; set; }
        public TimeSpan NewStartTime { get; set; }
    }

}
