using AppointmentSystem.Application.DTOs;

namespace AppointmentSystem.Application.Services
{
    public interface IBranchService
    {
        Task<List<BranchDto>> GetAllBranchesAsync();
        Task<BranchDto?> GetBranchByIdAsync(int id);
    }
}
