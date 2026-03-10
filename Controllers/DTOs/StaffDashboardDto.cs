namespace SalonApp.Controllers.DTOs
{
    public class StaffDashboardDto
    {
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public decimal TodayEarnings { get; set; }
        public List<StaffAppointmentDto> Appointments { get; set; }
    }
}
