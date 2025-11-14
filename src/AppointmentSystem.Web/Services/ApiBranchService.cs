using AppointmentSystem.Application.DTOs;
using AppointmentSystem.Application.Services;
using System.Net.Http.Json;

namespace AppointmentSystem.Web.Services
{
    public class ApiBranchService : IBranchService
    {
        private readonly HttpClient _httpClient;

        public ApiBranchService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<BranchDto>> GetAllBranchesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/branches");
                response.EnsureSuccessStatusCode();
                var branches = await response.Content.ReadFromJsonAsync<List<BranchDto>>();
                return branches ?? new List<BranchDto>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"API'ye bağlanılamadı: {ex.Message}. API adresi: {_httpClient.BaseAddress}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Şubeler yüklenirken hata oluştu: {ex.Message}", ex);
            }
        }

        public async Task<BranchDto?> GetBranchByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<BranchDto>($"/api/branches/{id}");
        }
    }
}

