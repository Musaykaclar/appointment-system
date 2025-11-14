using AppointmentSystem.Application.DTOs;

namespace AppointmentSystem.Application.Services
{
    public class BranchService : IBranchService
    {
        private readonly List<BranchDto> _branches = new()
        {
            new BranchDto { Id = 1, Name = "İstanbul Şube", Location = "İstanbul, Kadıköy" },
            new BranchDto { Id = 2, Name = "Ankara Şube", Location = "Ankara, Çankaya" },
            new BranchDto { Id = 3, Name = "İzmir Şube", Location = "İzmir, Konak" },
            new BranchDto { Id = 4, Name = "Bursa Şube", Location = "Bursa, Nilüfer" },
            new BranchDto { Id = 5, Name = "Antalya Şube", Location = "Antalya, Muratpaşa" },
        };

        public Task<List<BranchDto>> GetAllBranchesAsync()
        {
            return Task.FromResult(_branches);
        }

        public Task<BranchDto?> GetBranchByIdAsync(int id)
        {
            return Task.FromResult(_branches.FirstOrDefault(b => b.Id == id));
        }
    }
}
