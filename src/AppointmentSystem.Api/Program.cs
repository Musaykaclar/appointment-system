using AppointmentSystem.Application.DTOs;
using AppointmentSystem.Application.Services;
using AppointmentSystem.Application.Validators;
using AppointmentSystem.Domain.Entities;
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
var apiGroup = app.MapGroup("/api").WithTags("Appointments");

// Authentication endpoints
apiGroup.MapPost("/auth/login", async (
    LoginDto loginDto,
    IAuthService authService) =>
{
    var result = await authService.LoginAsync(loginDto);
    if (result.Success)
    {
        return Results.Ok(result);
    }
    return Results.Unauthorized();
}).WithName("Login");

apiGroup.MapPost("/auth/register", async (
    RegisterDto registerDto,
    IAuthService authService) =>
{
    var result = await authService.RegisterAsync(registerDto);
    if (result.Success)
    {
        return Results.Ok(result);
    }
    return Results.BadRequest(result);
}).WithName("Register");

apiGroup.MapGet("/auth/user/{id}", async (
    int id,
    IAuthService authService) =>
{
    var user = await authService.GetUserByIdAsync(id);
    return user != null ? Results.Ok(user) : Results.NotFound();
}).WithName("GetUserById");

// Branches
apiGroup.MapGet("/branches", async (IBranchService branchService) =>
{
    var branches = await branchService.GetAllBranchesAsync();
    return Results.Ok(branches);
}).WithName("GetBranches");

apiGroup.MapGet("/branches/{id}", async (int id, IBranchService branchService) =>
{
    var branch = await branchService.GetBranchByIdAsync(id);
    return branch != null ? Results.Ok(branch) : Results.NotFound();
}).WithName("GetBranchById");

// Appointments
// ÖNEMLİ: Route sıralaması önemli! Daha spesifik route'lar önce tanımlanmalı
// 1. Önce spesifik route'lar (approve, reject, audits, pending)
apiGroup.MapPost("/appointments/{id}/approve", async (
    int id,
    ApproveRequest request,
    IAppointmentService appointmentService) =>
{
    await appointmentService.ApproveAppointmentAsync(id, request.AdminUser);
    return Results.Ok(new { message = "Randevu onaylandı" });
}).WithName("ApproveAppointment");

apiGroup.MapPost("/appointments/{id}/reject", async (
    int id,
    RejectRequest request,
    IAppointmentService appointmentService) =>
{
    if (string.IsNullOrWhiteSpace(request.Comment))
    {
        return Results.BadRequest(new { error = "Red nedeni zorunludur" });
    }

    await appointmentService.RejectAppointmentAsync(id, request.AdminUser, request.Comment);
    return Results.Ok(new { message = "Randevu reddedildi" });
}).WithName("RejectAppointment");

apiGroup.MapGet("/appointments/{id}/audits", async (
    int id,
    IAppointmentService appointmentService) =>
{
    var audits = await appointmentService.GetAppointmentAuditsAsync(id);
    return Results.Ok(audits);
}).WithName("GetAppointmentAudits");

apiGroup.MapGet("/appointments/pending", async (
    [AsParameters] AppointmentFilterDto filter,
    IAppointmentService appointmentService) =>
{
    var result = await appointmentService.GetPendingAppointmentsAsync(filter);
    return Results.Ok(result);
}).WithName("GetPendingAppointments");

// 2. Sonra genel route'lar
apiGroup.MapGet("/appointments", async (
    [AsParameters] AppointmentFilterDto filter,
    IAppointmentService appointmentService) =>
{
    var result = await appointmentService.GetAppointmentsAsync(filter);
    return Results.Ok(result);
}).WithName("GetAppointments");

apiGroup.MapGet("/appointments/{id}", async (
    int id,
    IAppointmentService appointmentService) =>
{
    var appointment = await appointmentService.GetAppointmentByIdAsync(id);
    return appointment != null ? Results.Ok(appointment) : Results.NotFound();
}).WithName("GetAppointmentById");

apiGroup.MapPost("/appointments", async (
    AppointmentDto dto,
    IAppointmentService appointmentService,
    IValidator<AppointmentDto> validator,
    HttpContext httpContext) =>
{
    var validationResult = await validator.ValidateAsync(dto);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors);
    }

    // Token'dan userId'yi al (basit implementasyon)
    int? userId = null;
    if (httpContext.Request.Headers.TryGetValue("X-User-Id", out var userIdHeader))
    {
        if (int.TryParse(userIdHeader.ToString(), out var parsedUserId))
        {
            userId = parsedUserId;
            dto.RequestedById = userId;
        }
    }

    await appointmentService.CreateAppointmentAsync(dto, userId);
    
    // Oluşturulan randevuyu geri döndür
    var createdAppointment = await appointmentService.GetAppointmentByIdAsync(dto.Id);
    if (createdAppointment != null)
    {
        return Results.Created($"/api/appointments/{createdAppointment.Id}", createdAppointment);
    }
    
    return Results.Created($"/api/appointments/{dto.Id}", dto);
}).WithName("CreateAppointment");

apiGroup.MapPut("/appointments/{id}", async (
    int id,
    AppointmentDto dto,
    IAppointmentService appointmentService,
    IValidator<AppointmentDto> validator) =>
{
    if (id != dto.Id)
    {
        return Results.BadRequest("ID mismatch");
    }

    var validationResult = await validator.ValidateAsync(dto);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors);
    }

    await appointmentService.UpdateAppointmentAsync(dto);
    return Results.NoContent();
}).WithName("UpdateAppointment");

app.MapGet("/", () => "API Çalışıyor!");
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();

// Request DTOs
public record ApproveRequest(string AdminUser);
public record RejectRequest(string AdminUser, string Comment);
