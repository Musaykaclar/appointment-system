using AppointmentSystem.Application.DTOs;
using AppointmentSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Application.Services
{
    public class BranchService : IBranchService
    {
        private readonly AppointmentDbContext _context;

        public BranchService(AppointmentDbContext context)
        {
            _context = context;
        }

        public async Task<List<BranchDto>> GetAllBranchesAsync()
        {
            return await _context.Branches
                .Select(b => new BranchDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Location = b.Location
                })
                .ToListAsync();
        }

        public async Task<BranchDto?> GetBranchByIdAsync(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null) return null;

            return new BranchDto
            {
                Id = branch.Id,
                Name = branch.Name,
                Location = branch.Location
            };
        }
    }
}
