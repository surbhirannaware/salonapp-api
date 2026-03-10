using Microsoft.EntityFrameworkCore;
using SalonApp.Domain.Entities;

public static class DbSeeder
{
    public static async Task SeedAsync(SalonDbContext context)
    {
        Console.WriteLine("🚀 DbSeeder started");

        await context.Database.MigrateAsync();

        // =======================
        // ROLES
        // =======================
        if (!await context.Roles.AnyAsync())
        {
            context.Roles.AddRange(
                new Role { RoleName = "Admin" },
                new Role { RoleName = "Staff" },
                new Role { RoleName = "Customer" }
            );
            await context.SaveChangesAsync();
        }

        // =======================
        // ADMIN USER
        // =======================
        if (!await context.Users.AnyAsync(u => u.PhoneNumber == "9999999999"))
        {
            var admin = new User
            {
                FullName = "System Admin",
                PhoneNumber = "9999999999",              // ✅ Required
                Email = "admin@salon.com",                     // ✅ Optional
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                IsActive = true
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();

            var adminRole = await context.Roles
                .FirstAsync(r => r.RoleName == "Admin");

            context.UserRoles.Add(new UserRole
            {
                UserId = admin.UserId,
                RoleId = adminRole.RoleId
            });

            await context.SaveChangesAsync();
        }

        // =======================
        // SERVICE CATEGORIES
        // =======================
        if (!await context.ServiceCategories.AnyAsync())
        {
            context.ServiceCategories.AddRange(
                new ServiceCategory { CategoryName = "Hair", IsActive = true },
                new ServiceCategory { CategoryName = "Spa", IsActive = true }
            );

            await context.SaveChangesAsync();
        }

        // =======================
        // SERVICES
        // =======================
        if (!await context.Services.AnyAsync())
        {
            var hair = await context.ServiceCategories
                .FirstAsync(c => c.CategoryName == "Hair");

            var spa = await context.ServiceCategories
                .FirstAsync(c => c.CategoryName == "Spa");

            context.Services.AddRange(
                new Service
                {
                    CategoryId = hair.CategoryId,
                    ServiceName = "Hair Cut",
                    Price = 300,
                    DurationMinutes = 30,
                    IsActive = true
                },
                new Service
                {
                    CategoryId = spa.CategoryId,
                    ServiceName = "Facial",
                    Price = 800,
                    DurationMinutes = 60,
                    IsActive = true
                }
            );

            await context.SaveChangesAsync();
        }

        // =======================
        // STAFF USER
        // =======================
        if (!await context.Users.AnyAsync(u => u.PhoneNumber == "8888888888"))
        {
            var staffUser = new User
            {
                FullName = "Test Staff",
                PhoneNumber = "8888888888",              // ✅ Required
                Email = "staff@salon.com",                     // ✅ Optional
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
                IsActive = true
            };

            context.Users.Add(staffUser);
            await context.SaveChangesAsync();

            var staffRole = await context.Roles
                .FirstAsync(r => r.RoleName == "Staff");

            context.UserRoles.Add(new UserRole
            {
                UserId = staffUser.UserId,
                RoleId = staffRole.RoleId
            });

            await context.SaveChangesAsync();

            context.Staff.Add(new Staff
            {
                UserId = staffUser.UserId,
                Specialization = "Hair & Spa",
                IsActive = true
            });

            await context.SaveChangesAsync();
        }

        // =======================
        // STAFF SERVICES
        // =======================
        if (!await context.StaffServices.AnyAsync())
        {
            var staff = await context.Staff.FirstAsync();
            var services = await context.Services.ToListAsync();

            foreach (var service in services)
            {
                context.StaffServices.Add(new StaffService
                {
                    StaffId = staff.StaffId,
                    ServiceId = service.ServiceId
                });
            }

            await context.SaveChangesAsync();
        }

        Console.WriteLine("✅ DbSeeder finished");
    }
}