using AppointmentSystem.Application.DTOs;
using AppointmentSystem.Domain.Entities;

namespace AppointmentSystem.Application.Services
{
    public interface IAppointmentService
    {
        Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(AppointmentFilterDto filter);
        Task<PagedResult<AppointmentDto>> GetPendingAppointmentsAsync(AppointmentFilterDto filter);
        Task<AppointmentDto?> GetAppointmentByIdAsync(int id);
        Task<List<AppointmentAuditDto>> GetAppointmentAuditsAsync(int appointmentId);
        Task CreateAppointmentAsync(AppointmentDto dto, int? userId = null);
        Task UpdateAppointmentAsync(AppointmentDto dto);
        Task UpdateStatusAsync(int id, AppointmentStatus toStatus, string actionBy, string? comment = null);
        Task ApproveAppointmentAsync(int id, string adminUser);
        Task RejectAppointmentAsync(int id, string adminUser, string comment);
    }
}
