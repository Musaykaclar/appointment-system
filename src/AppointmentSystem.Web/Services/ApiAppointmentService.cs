using AppointmentSystem.Application.DTOs;
using AppointmentSystem.Application.Services;
using AppointmentSystem.Domain.Entities;
using System.Net.Http.Json;

namespace AppointmentSystem.Web.Services
{
    public class ApiAppointmentService : IAppointmentService
    {
        private readonly HttpClient _httpClient;

        public ApiAppointmentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(AppointmentFilterDto filter)
        {
            var queryParams = new List<string>();
            if (filter.Status.HasValue) queryParams.Add($"Status={(int)filter.Status.Value}");
            if (filter.BranchId.HasValue) queryParams.Add($"BranchId={filter.BranchId.Value}");
            if (filter.StartDate.HasValue) queryParams.Add($"StartDate={filter.StartDate.Value:yyyy-MM-dd}");
            if (filter.EndDate.HasValue) queryParams.Add($"EndDate={filter.EndDate.Value:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(filter.SearchText)) queryParams.Add($"SearchText={Uri.EscapeDataString(filter.SearchText)}");
            if (!string.IsNullOrWhiteSpace(filter.SortBy)) queryParams.Add($"SortBy={filter.SortBy}");
            if (filter.SortDescending.HasValue) queryParams.Add($"SortDescending={filter.SortDescending.Value}");
            if (filter.PageNumber.HasValue) queryParams.Add($"PageNumber={filter.PageNumber.Value}");
            if (filter.PageSize.HasValue) queryParams.Add($"PageSize={filter.PageSize.Value}");

            var url = "/api/appointments";
            if (queryParams.Any())
            {
                url += "?" + string.Join("&", queryParams);
            }
            
            var response = await _httpClient.GetFromJsonAsync<PagedResult<AppointmentDto>>(url);
            return response ?? new PagedResult<AppointmentDto>();
        }

        public async Task<PagedResult<AppointmentDto>> GetPendingAppointmentsAsync(AppointmentFilterDto filter)
        {
            var queryParams = new List<string>();
            if (filter.BranchId.HasValue) queryParams.Add($"BranchId={filter.BranchId.Value}");
            if (filter.StartDate.HasValue) queryParams.Add($"StartDate={filter.StartDate.Value:yyyy-MM-dd}");
            if (filter.EndDate.HasValue) queryParams.Add($"EndDate={filter.EndDate.Value:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(filter.SearchText)) queryParams.Add($"SearchText={Uri.EscapeDataString(filter.SearchText)}");
            if (!string.IsNullOrWhiteSpace(filter.SortBy)) queryParams.Add($"SortBy={Uri.EscapeDataString(filter.SortBy)}");
            if (filter.SortDescending.HasValue) queryParams.Add($"SortDescending={filter.SortDescending.Value}");
            if (filter.PageNumber.HasValue) queryParams.Add($"PageNumber={filter.PageNumber.Value}");
            if (filter.PageSize.HasValue) queryParams.Add($"PageSize={filter.PageSize.Value}");

            var url = "/api/appointments/pending";
            if (queryParams.Any())
            {
                url += "?" + string.Join("&", queryParams);
            }
            
            var response = await _httpClient.GetFromJsonAsync<PagedResult<AppointmentDto>>(url);
            return response ?? new PagedResult<AppointmentDto>();
        }

        public async Task<AppointmentDto?> GetAppointmentByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<AppointmentDto>($"/api/appointments/{id}");
        }

        public async Task<List<AppointmentAuditDto>> GetAppointmentAuditsAsync(int appointmentId)
        {
            var response = await _httpClient.GetFromJsonAsync<List<AppointmentAuditDto>>($"/api/appointments/{appointmentId}/audits");
            return response ?? new List<AppointmentAuditDto>();
        }

        public async Task CreateAppointmentAsync(AppointmentDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/appointments", dto);
            response.EnsureSuccessStatusCode();
            
            // Oluşturulan randevuyu response'dan al
            var createdAppointment = await response.Content.ReadFromJsonAsync<AppointmentDto>();
            if (createdAppointment != null && createdAppointment.Id > 0)
            {
                dto.Id = createdAppointment.Id;
            }
        }

        public async Task UpdateAppointmentAsync(AppointmentDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/appointments/{dto.Id}", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task ApproveAppointmentAsync(int id, string adminUser)
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/appointments/{id}/approve", new { AdminUser = adminUser });
            response.EnsureSuccessStatusCode();
        }

        public async Task RejectAppointmentAsync(int id, string adminUser, string comment)
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/appointments/{id}/reject", new { AdminUser = adminUser, Comment = comment });
            response.EnsureSuccessStatusCode();
        }
    }
}

