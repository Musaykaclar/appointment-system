using AppointmentSystem.Api.Endpoints;
using AppointmentSystem.Application.DTOs;
using AppointmentSystem.Application.Services;
using AppointmentSystem.Application.Validators;
using AppointmentSystem.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AppointmentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IValidator<AppointmentDto>, AppointmentValidator>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7236", 
                "http://localhost:5083",
                "http://localhost:5237",
                "https://localhost:7230",
                "https://localhost:7000", 
                "http://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Migrate + Seed
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppointmentDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Veritabanı migration ve seed işlemi başlatılıyor...");
        await DbInitializer.SeedAsync(db);
        logger.LogInformation("Veritabanı migration ve seed işlemi tamamlandı.");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı migration/seed sırasında hata oluştu!");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowBlazorClient");

// API Endpoints
app.MapAppointmentEndpoints();

// Health check endpoints
app.MapGet("/", () => "API Çalışıyor!");
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
