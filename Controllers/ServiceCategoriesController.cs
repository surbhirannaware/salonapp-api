using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonApp.Controllers.DTOs;
using SalonApp.Domain.Entities;

namespace SalonApp.Controllers
{
    [ApiController]
    [Route("api/service-categories")]
    [Authorize(Roles = "Admin")]
    public class ServiceCategoriesController : ControllerBase
    {
        private readonly SalonDbContext _db;

        public ServiceCategoriesController(SalonDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories(
            [FromQuery] string? search = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;

            var query = _db.ServiceCategories
                .Include(c => c.Services)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(c => c.CategoryName.ToLower().Contains(term));
            }

            var totalRecords = await query.CountAsync();

            var categories = await query
                .OrderBy(c => c.CategoryName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.CategoryId,
                    c.CategoryName,
                    c.IsActive,
                    ServiceCount = c.Services.Count
                })
                .ToListAsync();

            return Ok(new
            {
                Items = categories,
                Page = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _db.ServiceCategories
                .Where(c => c.CategoryId == id)
                .Select(c => new
                {
                    c.CategoryId,
                    c.CategoryName,
                    c.IsActive
                })
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound("Category not found.");

            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] AddCategoryDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.CategoryName))
                return BadRequest("Category name is required.");

            var categoryName = dto.CategoryName.Trim();

            if (categoryName.Length < 2 || categoryName.Length > 100)
                return BadRequest("Category name must be between 2 and 100 characters.");

            var exists = await _db.ServiceCategories
                .AnyAsync(c => c.CategoryName.ToLower() == categoryName.ToLower());

            if (exists)
                return BadRequest("Category already exists.");

            var category = new ServiceCategory
            {
                CategoryName = categoryName,
                IsActive = dto.IsActive
            };

            _db.ServiceCategories.Add(category);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                Message = "Category added successfully.",
                category.CategoryId,
                category.CategoryName,
                category.IsActive
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.CategoryName))
                return BadRequest("Category name is required.");

            var category = await _db.ServiceCategories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound("Category not found.");

            var categoryName = dto.CategoryName.Trim();

            if (categoryName.Length < 2 || categoryName.Length > 100)
                return BadRequest("Category name must be between 2 and 100 characters.");

            var duplicateExists = await _db.ServiceCategories
                .AnyAsync(c => c.CategoryId != id &&
                               c.CategoryName.ToLower() == categoryName.ToLower());

            if (duplicateExists)
                return BadRequest("Another category with same name already exists.");

            category.CategoryName = categoryName;
            category.IsActive = dto.IsActive;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                Message = "Category updated successfully.",
                category.CategoryId,
                category.CategoryName,
                category.IsActive
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _db.ServiceCategories
                .Include(c => c.Services)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound("Category not found.");

            if (category.Services.Any())
            {
                category.IsActive = false;
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Category has linked services, so it was deactivated instead of deleted."
                });
            }

            _db.ServiceCategories.Remove(category);
            await _db.SaveChangesAsync();

            return Ok(new { Message = "Category deleted successfully." });
        }

        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleCategoryStatus(int id)
        {
            var category = await _db.ServiceCategories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound("Category not found.");

            category.IsActive = !category.IsActive;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                Message = "Category status updated successfully.",
                category.CategoryId,
                category.CategoryName,
                category.IsActive
            });
        }
    }
}