using AppointmentSystem.Application.DTOs;
using AppointmentSystem.Application.Services;
using AppointmentSystem.Application.Validators;
using AppointmentSystem.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSystem.Api.Endpoints
{
    public static class AppointmentEndpoints
    {
        public static void MapAppointmentEndpoints(this IEndpointRouteBuilder app)
        {
            var apiGroup = app.MapGroup("/api").WithTags("Appointments");

            // Authentication endpoints
            MapAuthEndpoints(apiGroup);

            // Branch endpoints
            MapBranchEndpoints(apiGroup);

            // Appointment endpoints
            MapAppointmentRoutes(apiGroup);
        }

        private static void MapAuthEndpoints(RouteGroupBuilder apiGroup)
        {
            apiGroup.MapPost("/auth/login", async (
                LoginDto loginDto,
                IAuthService authService) =>
            {
                var result = await authService.LoginAsync(loginDto);
                return result.Success 
                    ? Results.Ok(result) 
                    : Results.Unauthorized();
            }).WithName("Login");

            apiGroup.MapPost("/auth/register", async (
                RegisterDto registerDto,
                IAuthService authService) =>
            {
                var result = await authService.RegisterAsync(registerDto);
                return result.Success 
                    ? Results.Ok(result) 
                    : Results.BadRequest(result);
            }).WithName("Register");

            apiGroup.MapGet("/auth/user/{id}", async (
                int id,
                IAuthService authService) =>
            {
                var user = await authService.GetUserByIdAsync(id);
                return user != null 
                    ? Results.Ok(user) 
                    : Results.NotFound();
            }).WithName("GetUserById");
        }

        private static void MapBranchEndpoints(RouteGroupBuilder apiGroup)
        {
            apiGroup.MapGet("/branches", async (IBranchService branchService) =>
            {
                var branches = await branchService.GetAllBranchesAsync();
                return Results.Ok(branches);
            }).WithName("GetBranches");

            apiGroup.MapGet("/branches/{id}", async (int id, IBranchService branchService) =>
            {
                var branch = await branchService.GetBranchByIdAsync(id);
                return branch != null 
                    ? Results.Ok(branch) 
                    : Results.NotFound();
            }).WithName("GetBranchById");
        }

        private static void MapAppointmentRoutes(RouteGroupBuilder apiGroup)
        {
            // ÖNEMLİ: Spesifik route'lar önce tanımlanmalı (route çakışmasını önlemek için)
            // /appointments/{id}/status endpoint'i /appointments/{id} GET'den ÖNCE olmalı
            
            // Spesifik route'lar önce (approve, reject, status, audits, pending)
            apiGroup.MapPost("/appointments/{id}/approve", async (
                int id,
                [FromBody] ApproveRequest request,
                IAppointmentService appointmentService) =>
            {
                try
                {
                    // Check if appointment exists
                    var appointment = await appointmentService.GetAppointmentByIdAsync(id);
                    if (appointment == null)
                    {
                        return Results.NotFound(new { error = $"Appointment with id {id} not found." });
                    }

                    await appointmentService.ApproveAppointmentAsync(id, request.AdminUser);
                    return Results.Ok(new { message = "Randevu onaylandı" });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            }).WithName("ApproveAppointment");

            apiGroup.MapPost("/appointments/{id}/reject", async (
                int id,
                [FromBody] RejectRequest request,
                IAppointmentService appointmentService) =>
            {
                if (string.IsNullOrWhiteSpace(request.Comment))
                {
                    return Results.BadRequest(new { error = "Red nedeni zorunludur" });
                }

                try
                {
                    // Check if appointment exists
                    var appointment = await appointmentService.GetAppointmentByIdAsync(id);
                    if (appointment == null)
                    {
                        return Results.NotFound(new { error = $"Appointment with id {id} not found." });
                    }

                    await appointmentService.RejectAppointmentAsync(id, request.AdminUser, request.Comment);
                    return Results.Ok(new { message = "Randevu reddedildi" });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            }).WithName("RejectAppointment");

            // Status update endpoint - /appointments/{id}/status
            // ÖNEMLİ: Bu endpoint /appointments/{id} GET endpoint'inden ÖNCE tanımlanmalı
            apiGroup.MapPost("/appointments/{id}/status", async (
                int id,
                [FromBody] UpdateStatusRequest request,
                IAppointmentService appointmentService) =>
            {
                try
                {
                    // Check if appointment exists
                    var appointment = await appointmentService.GetAppointmentByIdAsync(id);
                    if (appointment == null)
                    {
                        return Results.NotFound(new { error = $"Appointment with id {id} not found." });
                    }

                    await appointmentService.UpdateStatusAsync(id, request.ToStatus, request.ActionBy, request.Comment);
                    return Results.Ok(new { message = "Randevu durumu güncellendi" });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            })
            .WithName("UpdateAppointmentStatus")
            .WithTags("Appointments");

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

            // Genel route'lar - EN SON tanımlanmalı (route çakışmasını önlemek için)
            apiGroup.MapGet("/appointments", async (
                [AsParameters] AppointmentFilterDto filter,
                IAppointmentService appointmentService) =>
            {
                var result = await appointmentService.GetAppointmentsAsync(filter);
                return Results.Ok(result);
            }).WithName("GetAppointments");

            // /appointments/{id} GET endpoint'i EN SONA alındı (route çakışmasını önlemek için)
            apiGroup.MapGet("/appointments/{id}", async (
                int id,
                IAppointmentService appointmentService) =>
            {
                var appointment = await appointmentService.GetAppointmentByIdAsync(id);
                return appointment != null 
                    ? Results.Ok(appointment) 
                    : Results.NotFound();
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

                var userId = ExtractUserIdFromHeader(httpContext);
                if (userId.HasValue)
                {
                    dto.RequestedById = userId;
                }

                await appointmentService.CreateAppointmentAsync(dto, userId);
                
                var createdAppointment = await appointmentService.GetAppointmentByIdAsync(dto.Id);
                return createdAppointment != null
                    ? Results.Created($"/api/appointments/{createdAppointment.Id}", createdAppointment)
                    : Results.Created($"/api/appointments/{dto.Id}", dto);
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
        }

        private static int? ExtractUserIdFromHeader(HttpContext httpContext)
        {
            if (httpContext.Request.Headers.TryGetValue("X-User-Id", out var userIdHeader))
            {
                if (int.TryParse(userIdHeader.ToString(), out var parsedUserId))
                {
                    return parsedUserId;
                }
            }
            return null;
        }
    }

    // Request DTOs
    public record ApproveRequest(string AdminUser);
    public record RejectRequest(string AdminUser, string Comment);
    public record UpdateStatusRequest(AppointmentStatus ToStatus, string ActionBy, string? Comment = null);
}

