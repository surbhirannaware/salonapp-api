namespace SalonApp.Controllers.DTOs
{
    public class InvoiceServiceDto
    {
        public string ServiceName { get; set; } = null!;
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
    }

}
