using AppointmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppointmentDbContext context)
        {
            // Migration'ları çalıştır
            await context.Database.MigrateAsync();

            // Kullanıcılar
            if (!await context.Users.AnyAsync())
            {
                var users = new[]
                {
                    new User 
                    { 
                        Username = "admin", 
                        Password = "admin123", // Gerçek uygulamada hash'lenmiş olmalı
                        Email = "admin@example.com",
                        FullName = "Yönetici",
                        Role = UserRole.Admin,
                        IsActive = true
                    },
                    new User 
                    { 
                        Username = "user", 
                        Password = "user123", // Gerçek uygulamada hash'lenmiş olmalı
                        Email = "user@example.com",
                        FullName = "Kullanıcı",
                        Role = UserRole.User,
                        IsActive = true
                    }
                };
                context.Users.AddRange(users);
                await context.SaveChangesAsync();
                Console.WriteLine($"[DbInitializer] {users.Length} kullanıcı eklendi.");
            }
            else
            {
                var userCount = await context.Users.CountAsync();
                Console.WriteLine($"[DbInitializer] {userCount} kullanıcı zaten mevcut.");
            }

            // Şubeler
            if (!await context.Branches.AnyAsync())
            {
                var branches = new[]
                {
                    new Branch { Name = "İstanbul Şube", Location = "İstanbul, Kadıköy" },
                    new Branch { Name = "Ankara Şube", Location = "Ankara, Çankaya" },
                    new Branch { Name = "İzmir Şube", Location = "İzmir, Konak" },
                    new Branch { Name = "Bursa Şube", Location = "Bursa, Nilüfer" },
                    new Branch { Name = "Antalya Şube", Location = "Antalya, Muratpaşa" }
                };
                context.Branches.AddRange(branches);
                await context.SaveChangesAsync();
                Console.WriteLine($"[DbInitializer] {branches.Length} şube eklendi.");
            }
            else
            {
                var branchCount = await context.Branches.CountAsync();
                Console.WriteLine($"[DbInitializer] {branchCount} şube zaten mevcut.");
            }

            // Örnek Randevu
            if (!await context.Appointments.AnyAsync())
            {
                var branch = await context.Branches.OrderBy(b => b.Id).FirstOrDefaultAsync();
                if (branch == null)
                {
                    Console.WriteLine("[DbInitializer] UYARI: Şube bulunamadı, örnek randevu oluşturulamadı!");
                    return;
                }
                
                var appointmentDate = DateTime.UtcNow.AddDays(2).Date;
                var appointment = new Appointment
                {
                    Title = "Örnek Randevu",
                    BranchId = branch.Id,
                    Date = DateTime.SpecifyKind(appointmentDate, DateTimeKind.Utc),
                    StartTime = new TimeSpan(10, 0, 0),
                    EndTime = new TimeSpan(11, 0, 0),
                    RequestedBy = "Kullanıcı",
                    Description = "Örnek randevu açıklaması",
                    Status = AppointmentStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                context.Appointments.Add(appointment);
                await context.SaveChangesAsync();
                Console.WriteLine($"[DbInitializer] Örnek randevu eklendi (ID: {appointment.Id}).");

                context.AppointmentAudits.Add(new AppointmentAudit
                {
                    AppointmentId = appointment.Id,
                    FromStatus = AppointmentStatus.Draft,
                    ToStatus = AppointmentStatus.Pending,
                    ActionBy = "Kullanıcı",
                    ActionAt = DateTime.UtcNow,
                    Comment = "Randevu talebi gönderildi"
                });
                await context.SaveChangesAsync();
                Console.WriteLine("[DbInitializer] Örnek randevu audit kaydı eklendi.");
            }
            else
            {
                var appointmentCount = await context.Appointments.CountAsync();
                Console.WriteLine($"[DbInitializer] {appointmentCount} randevu zaten mevcut.");
            }
        }
    }
}
