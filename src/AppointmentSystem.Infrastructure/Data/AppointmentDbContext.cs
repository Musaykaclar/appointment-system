using AppointmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;



namespace AppointmentSystem.Infrastructure.Data
{
    public class AppointmentDbContext : DbContext
    {
        public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options)
            : base(options)
        {
        }

        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<AppointmentAudit> AppointmentAudits => Set<AppointmentAudit>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Appointment
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Title).IsRequired().HasMaxLength(200);

                entity.HasMany(x => x.Audits)
                      .WithOne()
                      .HasForeignKey(a => a.AppointmentId);
            });

            // Branch
            modelBuilder.Entity<Branch>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).HasMaxLength(100);
            });

            // AppointmentAudit
            modelBuilder.Entity<AppointmentAudit>(entity =>
            {
                entity.HasKey(x => x.Id);
            });
        }
    }
}
