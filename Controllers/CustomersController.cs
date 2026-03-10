using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SalonApp.Controllers
{
    [ApiController]
    [Route("api/admin/customers")]
    public class CustomersController : ControllerBase
    {
        private readonly SalonDbContext _db;

        public CustomersController(SalonDbContext db)
        {
            _db = db;
        }
         
        [HttpGet("search")]
        public async Task<IActionResult> SearchCustomers(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Ok(new List<object>());

            var customers = await (
                from u in _db.Users
                join ur in _db.UserRoles on u.UserId equals ur.UserId
                join r in _db.Roles on ur.RoleId equals r.RoleId
                where r.RoleName == "Customer"
 && u.FullName.ToLower().StartsWith(term.ToLower())
                select new
                {
                    customerId = u.UserId,
                    name = u.FullName,
                    email = u.Email,
                    phone = u.PhoneNumber
                }
            )
            .OrderBy(x => x.name)
            .Take(10)
            .ToListAsync();

            return Ok(customers);
        }

    }
}
