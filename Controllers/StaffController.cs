using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SalonApp.Controllers
{
    [ApiController]
    [Route("api/staff")]
    [Authorize]
    public class StaffController : ControllerBase
    {
        private readonly SalonDbContext _db;

        public StaffController(SalonDbContext db)
        {
            _db = db;
        }


       
    }

}
