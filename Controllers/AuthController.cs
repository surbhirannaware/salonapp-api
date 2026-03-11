
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonApp.Controllers.DTOs;
using SalonApp.Domain.Entities;
using System.Security.Claims;
using System.Text.RegularExpressions;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly SalonDbContext _db;
    private readonly JwtTokenService _jwt;

    public AuthController(SalonDbContext db, JwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    // REGISTER
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _db.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNo))
            return BadRequest("Phone number already exists");

        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = string.IsNullOrWhiteSpace(request.Email)
                            ? null
                            : request.Email.Trim().ToLower(),
                PhoneNumber = request.PhoneNo.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            string roleToAssign;

            // 🔹 Check if token exists
            if (User?.Identity?.IsAuthenticated == true)
            {
                var loggedInRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (loggedInRole != "Admin")
                    return Forbid("Only Admin can create staff");

                roleToAssign = "Staff";
            }
            else
            {
                roleToAssign = "Customer";
            }

            var role = await _db.Roles
                .FirstOrDefaultAsync(r => r.RoleName == roleToAssign);

            if (role == null)
                return StatusCode(500, "Role not found in database");

            _db.UserRoles.Add(new UserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId
            });

            // 🔹 If staff → insert into Staff table
            if (roleToAssign == "Staff")
            {
                _db.Staff.Add(new Staff
                {
                    UserId = user.UserId,
                    Specialization = "General",
                    IsActive = true
                });
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok($"{roleToAssign} registered successfully");
        }
        catch
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Registration failed");
        }
    }
    // LOGIN
    [HttpPost("login")]
    
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            Console.WriteLine("LOGIN STEP 1: request received for " + request.PhoneNo);

            var user = await _db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNo);

            Console.WriteLine("LOGIN STEP 2: user found = " + (user != null));

            if (user == null)
                return Unauthorized("Invalid credentials");

            var passwordOk = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            Console.WriteLine("LOGIN STEP 3: password verified = " + passwordOk);

            if (!passwordOk)
                return Unauthorized("Invalid credentials");

            var roles = user.UserRoles
                .Where(r => r.Role != null)
                .Select(r => r.Role.RoleName)
                .ToList();


            Console.WriteLine("LOGIN STEP 4: roles count = " + roles.Count);

            var token = _jwt.GenerateToken(user, roles);

            Console.WriteLine("LOGIN STEP 5: token generated successfully");

            return Ok(new AuthResponse
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("LOGIN ERROR: " + ex);
            return StatusCode(500, new
            {
                message = ex.Message,
                inner = ex.InnerException?.Message,
                stack = ex.StackTrace
            });
        }
    }
    //public async Task<IActionResult> Login(LoginRequest request)
    //{
    //    var user = await _db.Users
    //        .Include(u => u.UserRoles)
    //        .ThenInclude(ur => ur.Role)
    //        .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNo);

    //    if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
    //        return Unauthorized("Invalid credentials");

    //    var roles = user.UserRoles.Select(r => r.Role.RoleName).ToList();
    //    var token = _jwt.GenerateToken(user, roles);

    //    return Ok(new AuthResponse
    //    {
    //        Token = token,
    //        ExpiresAt = DateTime.UtcNow.AddMinutes(60)
    //    });
    //}
}
