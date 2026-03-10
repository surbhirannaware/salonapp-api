using System.ComponentModel.DataAnnotations;

namespace SalonApp.Controllers.DTOs
{
    public class UpdateServiceRequest
    {
            [Required]
            [MaxLength(100)]
            public string ServiceName { get; set; } = null!;

            [MaxLength(500)]
            public string? Description { get; set; }

            [Required]
            public int CategoryId { get; set; }

            [Range(1, 100000)]
            public decimal Price { get; set; }

            [Range(1, 600)]
            public int DurationMinutes { get; set; }
        }
}
