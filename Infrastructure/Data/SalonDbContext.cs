using Microsoft.EntityFrameworkCore;
using SalonApp.Domain.Entities;

public class SalonDbContext : DbContext
{
    public SalonDbContext(DbContextOptions<SalonDbContext> options)
        : base(options)
    {
    }



    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<StaffService> StaffServices => Set<StaffService>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentService> AppointmentServices => Set<AppointmentService>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<StaffAvailability> StaffAvailabilities { get; set; }

    public DbSet<StaffLeave> StaffLeaves => Set<StaffLeave>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========================
        // USERS
        // ========================
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.UserId);

            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .IsRequired();

            //entity.Property(e => e.Email)
            //    .HasMaxLength(150)
            //    .IsRequired();

            //entity.HasIndex(e => e.Email)
            //    .IsUnique();

            entity.HasIndex(u => u.PhoneNumber).IsUnique();


            entity.Property(e => e.PasswordHash)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            //entity.Property(e => e.CreatedAt)
            //    .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.CreatedAt)
    .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // ========================
        // ROLES
        // ========================
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.RoleId);

            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsRequired();
        });

        // ========================
        // USER ROLES
        // ========================
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(e => e.UserRoleId);

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId);
        });

        // ========================
        // SERVICE CATEGORIES
        // ========================
        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.ToTable("ServiceCategories");
            entity.HasKey(e => e.CategoryId);

            entity.Property(e => e.CategoryName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);
        });

        // ========================
        // SERVICES
        // ========================
        modelBuilder.Entity<Service>(entity =>
        {
            entity.ToTable("Services");
            entity.HasKey(e => e.ServiceId);

            entity.Property(e => e.ServiceName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.Price)
                .HasColumnType("decimal(10,2)");

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Services)
                .HasForeignKey(e => e.CategoryId);
        });

        // ========================
        // STAFF
        // ========================
        modelBuilder.Entity<Staff>(entity =>
        {
            entity.ToTable("Staff");
            entity.HasKey(e => e.StaffId);

            entity.Property(e => e.Specialization)
                .HasMaxLength(100);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId);
        });

        // ========================
        // STAFF SERVICES
        // ========================
        modelBuilder.Entity<StaffService>(entity =>
        {
            entity.ToTable("StaffServices");
            entity.HasKey(e => e.StaffServiceId);

            entity.HasOne(e => e.Staff)
                .WithMany(s => s.StaffServices)
                .HasForeignKey(e => e.StaffId);

            entity.HasOne(e => e.Service)
                .WithMany(s => s.StaffServices)
                .HasForeignKey(e => e.ServiceId);
        });

        // ========================
        // APPOINTMENTS
        // ========================
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("Appointments");
            entity.HasKey(e => e.AppointmentId);

            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.AppointmentDate);

            entity.HasOne(e => e.Staff)
                .WithMany(s => s.Appointments)
                .HasForeignKey(e => e.StaffId);

            entity.HasIndex(a => new { a.StaffId, a.AppointmentDate, a.StartTime })
                .IsUnique();

            entity.HasOne(a => a.CreatedByUser)
                .WithMany(u => u.CreatedAppointments)
                .HasForeignKey(a => a.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.CustomerUser)
                .WithMany(u => u.CustomerAppointments)
                .HasForeignKey(a => a.CustomerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Payment)
                .WithOne(p => p.Appointment)
                .HasForeignKey<Payment>(p => p.AppointmentId);
        });

        //    modelBuilder.Entity<Appointment>(entity =>
        //    {
        //        entity.ToTable("Appointments");


        //        // entity.ToTable(tb => tb.HasTrigger("TR_BlockOverlappingAppointments"));
        //        //entity.HasKey(e => e.AppointmentId);

        //        entity.Property(e => e.Status)
        //            .HasMaxLength(30)
        //            .IsRequired();

        //        //entity.Property(e => e.CreatedAt)
        //        //    .HasDefaultValueSql("GETUTCDATE()");

        //        entity.Property(e => e.CreatedAt)
        //             .HasDefaultValueSql("CURRENT_TIMESTAMP");

        //        entity.HasIndex(e => e.AppointmentDate);

        //        //entity.HasOne(e => e.CreatedByUser)
        //        //    .WithMany(u => u.Appointments)
        //        //    .HasForeignKey(e => e.CreatedByUserId);

        //        entity.HasOne(e => e.Staff)
        //            .WithMany(s => s.Appointments)
        //            .HasForeignKey(e => e.StaffId);

        //        entity.HasIndex(a => new { a.StaffId, a.AppointmentDate, a.StartTime })
        //            .IsUnique();

        //        entity.HasOne(a => a.Payment)
        //        .WithOne(p => p.Appointment)
        //        .HasForeignKey<Payment>(p => p.AppointmentId);


        //    entity.HasOne(a => a.CreatedByUser)
        //    .WithMany()
        //    .HasForeignKey(a => a.CreatedByUserId)
        //    .OnDelete(DeleteBehavior.Restrict);


        //            //entity.HasOne(a => a.CustomerUser)
        //            //.WithMany(u => u.Appointments)
        //            //.HasForeignKey(a => a.CustomerUserId)
        //            //.OnDelete(DeleteBehavior.Restrict);


        //        entity.HasOne(a => a.CreatedByUser)
        //.WithMany(u => u.CreatedAppointments)
        //.HasForeignKey(a => a.CreatedByUserId)
        //.OnDelete(DeleteBehavior.Restrict);


        //           entity.HasOne(a => a.CustomerUser)
        //            .WithMany(u => u.CustomerAppointments)
        //            .HasForeignKey(a => a.CustomerUserId)
        //            .OnDelete(DeleteBehavior.Restrict);

        //    });

        // ========================
        // APPOINTMENT SERVICES
        // ========================
        modelBuilder.Entity<AppointmentService>(entity =>
        {
            entity.ToTable("AppointmentServices");
            entity.HasKey(e => e.AppointmentServiceId);

            entity.Property(e => e.PriceAtBooking)
                .HasColumnType("decimal(10,2)");

            entity.HasOne(e => e.Appointment)
                .WithMany(a => a.AppointmentServices)
                .HasForeignKey(e => e.AppointmentId);

            entity.HasOne(e => e.Service)
                .WithMany(s => s.AppointmentServices)
                .HasForeignKey(e => e.ServiceId);
        });

        // ========================
        // PAYMENTS
        // ========================
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(e => e.PaymentId);

            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10,2)");

            entity.Property(e => e.TransactionId)
                .HasMaxLength(200);

            entity.HasOne(p => p.Appointment)
        .WithOne(a => a.Payment)
        .HasForeignKey<Payment>(p => p.AppointmentId);


             

          

        });

        //=========================
        // Staff Availability
        //=========================
        modelBuilder.Entity<StaffAvailability>(entity =>
        {
            entity.HasKey(e => e.AvailabilityId);

            entity.HasOne(e => e.Staff)
                  .WithMany()
                  .HasForeignKey(e => e.StaffId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.StaffId, e.DayOfWeek })
                  .IsUnique();

            entity.Property(sa => sa.DayOfWeek).IsRequired();

        });

        //=========================
        // Staff Leave
        //=========================
        modelBuilder.Entity<StaffLeave>(entity =>
        {
            entity.ToTable("StaffLeaves");
            entity.HasKey(e => e.StaffLeaveId);

            entity.Property(e => e.Reason)
          .HasMaxLength(200);

            entity.HasOne(e => e.Staff)
                .WithMany()
                .HasForeignKey(e => e.StaffId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.StaffId, e.LeaveDate });
        });
    }
}
