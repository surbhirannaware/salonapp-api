namespace SalonApp.Controllers.DTOs
{
    public class AddCategoryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}