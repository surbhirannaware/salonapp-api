using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonApp.Controllers.DTOs;
using SalonApp.Domain.Entities;

namespace SalonApp.Controllers
{

    [ApiController]
    [Route("api/services")]
    public class ServicesController : ControllerBase
    {
        private readonly SalonDbContext _db;

        public ServicesController(SalonDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetServices(
     [FromQuery] string? search = "",
     [FromQuery] int? categoryId = null,
     [FromQuery] int page = 1,
     [FromQuery] int pageSize = 6,
     [FromQuery] bool includeInactive = false)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 6;

            if (includeInactive && !User.IsInRole("Admin"))
            {
                includeInactive = false;
            }

            var query = _db.Services
                .Include(s => s.Category)
                .Where(s => s.Category.IsActive)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(s => s.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(s => s.ServiceName.ToLower().Contains(term));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(s => s.CategoryId == categoryId.Value);
            }

            var totalRecords = await query.CountAsync();

            var services = await query
                .OrderBy(s => s.Category.CategoryName)
                .ThenBy(s => s.ServiceName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new
                {
                    serviceId = s.ServiceId,
                    serviceName = s.ServiceName,
                    description = s.Description,
                    categoryId = s.CategoryId,
                    categoryName = s.Category.CategoryName,
                    price = s.Price,
                    durationMinutes = s.DurationMinutes,
                    isActive = s.IsActive
                })
                .ToListAsync();

            return Ok(new
            {
                items = services,
                page,
                pageSize,
                totalRecords,
                totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
            });
        }


        // ✅ GET Categories (for dropdown)
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _db.ServiceCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .Select(c => new
                {
                    c.CategoryId,
                    c.CategoryName
                })
                .ToListAsync();

            return Ok(categories);
        }

        // ✅ POST Add Service
        [HttpPost]
        public async Task<IActionResult> AddService(AddServiceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Trim inputs first
            dto.ServiceName = dto.ServiceName?.Trim();
            dto.Description = dto.Description?.Trim();

            // Category validation
            if (dto.CategoryId == null)
                return BadRequest("Category is required.");

            var categoryExists = await _db.ServiceCategories
                .AnyAsync(c => c.CategoryId == dto.CategoryId && c.IsActive);

            if (!categoryExists)
                return BadRequest("Invalid or inactive category selected.");

            // Service Name validation
            if (string.IsNullOrWhiteSpace(dto.ServiceName))
                return BadRequest("Service name is required.");

            if (dto.ServiceName.Length < 3 || dto.ServiceName.Length > 100)
                return BadRequest("Service name must be between 3 and 100 characters.");

            var exists = await _db.Services
            .AnyAsync(s => s.ServiceName.ToLower() == dto.ServiceName.ToLower()
                && s.CategoryId == dto.CategoryId);

            if (exists)
                return BadRequest("Service already exists in this category.");

            // Description
            if (!string.IsNullOrEmpty(dto.Description) &&
                dto.Description.Length > 500)
            {
                return BadRequest("Description cannot exceed 500 characters.");
            }

            // Price validation
            if (dto.Price <= 0)
                return BadRequest("Price must be a positive number.");

            if (dto.Price > 50000)
                return BadRequest("Price exceeds allowed limit.");

            // Duration validation
            if (dto.DurationMinutes < 5)
                return BadRequest("Duration must be between 5 and 240 minutes.");

            if (dto.DurationMinutes > 240)
                return BadRequest("Duration cannot exceed 240 minutes.");

            var service = new Service
            {
                CategoryId = dto.CategoryId,
                ServiceName = dto.ServiceName,
                Description = dto.Description,
                Price = dto.Price,
                DurationMinutes = dto.DurationMinutes,
                IsActive = dto.IsActive
            };

            _db.Services.Add(service);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Service added successfully",
                serviceId = service.ServiceId
            });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetServiceById(int id)
        {
            var service = await _db.Services
                .Where(s => s.ServiceId == id)
                .Select(s => new
                {
                    s.ServiceId,
                    s.ServiceName,
                    s.Description,
                    s.CategoryId,
                    s.Price,
                    s.DurationMinutes,
                    s.IsActive
                })
                .FirstOrDefaultAsync();

            if (service == null)
                return NotFound("Service not found");

            return Ok(service);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateService(int id, UpdateServiceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var service = await _db.Services.FindAsync(id);

            if (service == null)
                return NotFound("Service not found");

            var categoryExists = await _db.ServiceCategories
                .AnyAsync(c => c.CategoryId == request.CategoryId && c.IsActive);

            if (!categoryExists)
                return BadRequest("Invalid Category");

            service.ServiceName = request.ServiceName.Trim();
            service.Description = request.Description;
            service.CategoryId = request.CategoryId;
            service.Price = request.Price;
            service.DurationMinutes = request.DurationMinutes;

            await _db.SaveChangesAsync();

            return Ok("Service updated successfully");
        }

        [HttpPut("{id}/toggle")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleService(int id)
        {
            var service = await _db.Services.FindAsync(id);

            if (service == null)
                return NotFound("Service not found");

            service.IsActive = !service.IsActive;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                service.ServiceId,
                service.IsActive
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _db.Services.FindAsync(id);

            if (service == null)
                return NotFound("Service not found");

            service.IsActive = false;

            await _db.SaveChangesAsync();

            return Ok("Service deleted successfully");
        }
    }
}
