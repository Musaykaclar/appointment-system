using AppointmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppointmentDbContext context)
        {
            await context.Database.MigrateAsync();

            // Şubeler
            if (!await context.Branches.AnyAsync())
            {
                context.Branches.AddRange(new[]
                {
                    new Branch { Name = "Merkez", Location = "İstanbul" },
                    new Branch { Name = "Şube 1", Location = "İstanbul" },
                    new Branch { Name = "Şube 2", Location = "İstanbul" },
                    new Branch { Name = "Şube 3", Location = "İstanbul" },
                    new Branch { Name = "Şube 4", Location = "İstanbul" }
                });
            }

            // Örnek Randevu
            if (!await context.Appointments.AnyAsync())
            {
                context.Appointments.Add(new Appointment
                {
                    Title = "Örnek Randevu",
                    BranchId = 1,
                    Date = DateTime.Now.AddDays(2),
                    StartTime = new TimeSpan(10, 0, 0),
                    EndTime = new TimeSpan(11, 0, 0),
                    RequestedBy = "user1",
                    Status = AppointmentStatus.Pending
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
