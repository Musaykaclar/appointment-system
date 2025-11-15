using AppointmentSystem.Application.DTOs;
using AppointmentSystem.Application.Services;
using AppointmentSystem.Web.Helpers;
using System.Net.Http.Json;

namespace AppointmentSystem.Web.Services
{
    public class ApiAppointmentService : IAppointmentService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthStateService? _authState;

        public ApiAppointmentService(HttpClient httpClient, AuthStateService? authState = null)
        {
            _httpClient = httpClient;
            _authState = authState;
        }

        private void AddAuthHeaders()
        {
            if (_authState?.CurrentUser != null)
            {
                _httpClient.DefaultRequestHeaders.Remove("X-User-Id");
                _httpClient.DefaultRequestHeaders.Add("X-User-Id", _authState.CurrentUser.Id.ToString());
            }
        }

        public async Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(AppointmentFilterDto filter)
        {
            var queryString = QueryStringHelper.BuildQueryString(filter);
            var url = $"/api/appointments{queryString}";
            
            var response = await _httpClient.GetFromJsonAsync<PagedResult<AppointmentDto>>(url);
            return response ?? new PagedResult<AppointmentDto>();
        }

        public async Task<PagedResult<AppointmentDto>> GetPendingAppointmentsAsync(AppointmentFilterDto filter)
        {
            var queryString = QueryStringHelper.BuildQueryString(filter);
            var url = $"/api/appointments/pending{queryString}";
            
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

        public async Task CreateAppointmentAsync(AppointmentDto dto, int? userId = null)
        {
            AddAuthHeaders();
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
            AddAuthHeaders();
            var response = await _httpClient.PostAsJsonAsync($"/api/appointments/{id}/approve", new { AdminUser = adminUser });
            response.EnsureSuccessStatusCode();
        }

        public async Task RejectAppointmentAsync(int id, string adminUser, string comment)
        {
            AddAuthHeaders();
            var response = await _httpClient.PostAsJsonAsync($"/api/appointments/{id}/reject", new { AdminUser = adminUser, Comment = comment });
            response.EnsureSuccessStatusCode();
        }
    }
}

