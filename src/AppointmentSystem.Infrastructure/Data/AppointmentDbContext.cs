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
                entity.Property(x => x.RequestedBy).IsRequired().HasMaxLength(100);
                entity.Property(x => x.Description).HasMaxLength(500);
                entity.Property(x => x.AdminComment).HasMaxLength(500);
                
                // Date alanını UTC'ye çevir
                entity.Property(x => x.Date)
                      .HasConversion(
                          v => v.ToUniversalTime(),
                          v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

                entity.HasOne(x => x.Branch)
                      .WithMany()
                      .HasForeignKey(x => x.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(x => x.Audits)
                      .WithOne(a => a.Appointment)
                      .HasForeignKey(a => a.AppointmentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Branch
            modelBuilder.Entity<Branch>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired().HasMaxLength(100);
                entity.Property(x => x.Location).IsRequired().HasMaxLength(200);
            });

            // AppointmentAudit
            modelBuilder.Entity<AppointmentAudit>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.ActionBy).IsRequired().HasMaxLength(100);
                entity.Property(x => x.Comment).HasMaxLength(500);
            });
        }
    }
}
