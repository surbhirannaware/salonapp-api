using System.ComponentModel.DataAnnotations;

namespace SalonApp.Controllers.DTOs
{
    public class AddServiceDto
    {
        [Required]
        public int CategoryId { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string ServiceName { get; set; } = null!;
        
        [StringLength(500)]
        public string? Description { get; set; }

        [Range(1, 100000)]
        public decimal Price { get; set; }

        [Range(5, 480)]
        public int DurationMinutes { get; set; }
        public bool IsActive { get; set; } = true;
  
          
    }
}
