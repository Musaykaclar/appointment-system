using AppointmentSystem.Application.Constants;
using AppointmentSystem.Application.DTOs;
using AppointmentSystem.Domain.Entities;
using AppointmentSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly AppointmentDbContext _context;

        public AppointmentService(AppointmentDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(AppointmentFilterDto filter)
        {
            var query = _context.Appointments
                .Include(a => a.Branch)
                .AsQueryable();

            // Filtreleme
            query = ApplyFilters(query, filter);

            // Sıralama
            query = ApplySorting(query, filter);

            var totalCount = await query.CountAsync();
            
            // Pagination için varsayılan değerler
            var pageNumber = filter.PageNumber ?? AppointmentConstants.DefaultPageNumber;
            var pageSize = filter.PageSize ?? AppointmentConstants.DefaultPageSize;
            
            var items = await GetAppointmentDtosAsync(query, pageNumber, pageSize);

            return new PagedResult<AppointmentDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<AppointmentDto>> GetPendingAppointmentsAsync(AppointmentFilterDto filter)
        {
            filter.Status = AppointmentStatus.Pending;
            return await GetAppointmentsAsync(filter);
        }

        public async Task<AppointmentDto?> GetAppointmentByIdAsync(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Branch)
                .Include(a => a.RequestedByUser)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return null;

            return MapToAppointmentDto(appointment);
        }

        public async Task<List<AppointmentAuditDto>> GetAppointmentAuditsAsync(int appointmentId)
        {
            return await _context.AppointmentAudits
                .Where(a => a.AppointmentId == appointmentId)
                .OrderByDescending(a => a.ActionAt)
                .Select(a => new AppointmentAuditDto
                {
                    AppointmentId = a.AppointmentId,
                    FromStatus = a.FromStatus,
                    ToStatus = a.ToStatus,
                    ActionBy = a.ActionBy,
                    ActionAt = a.ActionAt,
                    Comment = a.Comment
                })
                .ToListAsync();
        }

        public async Task CreateAppointmentAsync(AppointmentDto dto, int? userId = null)
        {
            var appointment = new Appointment
            {
                BranchId = dto.BranchId,
                RequestedBy = dto.RequestedBy,
                RequestedById = userId,
                Title = dto.Title,
                Date = dto.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Description = dto.Description,
                Status = AppointmentStatus.Pending, // Gönder butonu ile Pending durumuna çekilir
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Oluşturulan appointment'ın Id'sini dto'ya yaz
            dto.Id = appointment.Id;

            // Audit kaydı
            AddAuditRecord(appointment.Id, AppointmentStatus.Draft, AppointmentStatus.Pending, dto.RequestedBy, AppointmentConstants.AuditCommentCreated);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAppointmentAsync(AppointmentDto dto)
        {
            var existing = await _context.Appointments.FindAsync(dto.Id);
            if (existing != null)
            {
                var oldStatus = existing.Status;
                existing.Title = dto.Title;
                existing.Date = dto.Date;
                existing.StartTime = dto.StartTime;
                existing.EndTime = dto.EndTime;
                existing.BranchId = dto.BranchId;
                existing.Description = dto.Description;
                existing.Status = dto.Status;
                existing.UpdatedAt = DateTime.UtcNow;

                if (oldStatus != dto.Status)
                {
                    AddAuditRecord(dto.Id, oldStatus, dto.Status, dto.RequestedBy, AppointmentConstants.AuditCommentUpdated);
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task ApproveAppointmentAsync(int id, string adminUser)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null && appointment.Status == AppointmentStatus.Pending)
            {
                var oldStatus = appointment.Status;
                appointment.Status = AppointmentStatus.Approved;
                appointment.UpdatedAt = DateTime.UtcNow;

                AddAuditRecord(id, oldStatus, AppointmentStatus.Approved, adminUser, AppointmentConstants.AuditCommentApproved);

                await _context.SaveChangesAsync();
            }
        }

        public async Task RejectAppointmentAsync(int id, string adminUser, string comment)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null && appointment.Status == AppointmentStatus.Pending)
            {
                var oldStatus = appointment.Status;
                appointment.Status = AppointmentStatus.Rejected;
                appointment.AdminComment = comment;
                appointment.UpdatedAt = DateTime.UtcNow;

                _context.AppointmentAudits.Add(new AppointmentAudit
                {
                    AppointmentId = id,
                    FromStatus = oldStatus,
                    ToStatus = AppointmentStatus.Rejected,
                    ActionBy = adminUser,
                    ActionAt = DateTime.UtcNow,
                    Comment = comment
                });

                await _context.SaveChangesAsync();
            }
        }

        // Private helper methods
        private IQueryable<Appointment> ApplyFilters(IQueryable<Appointment> query, AppointmentFilterDto filter)
        {
            if (filter.Status.HasValue)
            {
                query = query.Where(a => a.Status == filter.Status.Value);
            }

            if (filter.BranchId.HasValue)
            {
                query = query.Where(a => a.BranchId == filter.BranchId.Value);
            }

            if (filter.RequestedById.HasValue)
            {
                query = query.Where(a => a.RequestedById == filter.RequestedById.Value);
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(a => a.Date >= filter.StartDate.Value.Date);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(a => a.Date <= filter.EndDate.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var search = filter.SearchText.ToLower();
                query = query.Where(a => 
                    a.Title.ToLower().Contains(search) || 
                    a.RequestedBy.ToLower().Contains(search));
            }

            return query;
        }

        private IQueryable<Appointment> ApplySorting(IQueryable<Appointment> query, AppointmentFilterDto filter)
        {
            var sortDescending = filter.SortDescending ?? false;
            return filter.SortBy?.ToLower() switch
            {
                AppointmentConstants.SortByDate => sortDescending 
                    ? query.OrderByDescending(a => a.Date) 
                    : query.OrderBy(a => a.Date),
                AppointmentConstants.SortByStatus => sortDescending 
                    ? query.OrderByDescending(a => a.Status) 
                    : query.OrderBy(a => a.Status),
                AppointmentConstants.SortByRequestedBy => sortDescending
                    ? query.OrderByDescending(a => a.RequestedBy)
                    : query.OrderBy(a => a.RequestedBy),
                _ => query.OrderByDescending(a => a.Date)
            };
        }

        private async Task<List<AppointmentDto>> GetAppointmentDtosAsync(IQueryable<Appointment> query, int pageNumber, int pageSize)
        {
            return await query
                .Include(a => a.RequestedByUser)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    BranchId = a.BranchId,
                    BranchName = a.Branch.Name,
                    BranchLocation = a.Branch.Location,
                    RequestedBy = a.RequestedByUser != null ? a.RequestedByUser.FullName : a.RequestedBy,
                    RequestedById = a.RequestedById,
                    Title = a.Title,
                    Date = a.Date,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Description = a.Description,
                    Status = a.Status,
                    AdminComment = a.AdminComment
                })
                .ToListAsync();
        }

        private AppointmentDto MapToAppointmentDto(Appointment appointment)
        {
            return new AppointmentDto
            {
                Id = appointment.Id,
                BranchId = appointment.BranchId,
                BranchName = appointment.Branch.Name,
                BranchLocation = appointment.Branch.Location,
                RequestedBy = appointment.RequestedByUser != null ? appointment.RequestedByUser.FullName : appointment.RequestedBy,
                RequestedById = appointment.RequestedById,
                Title = appointment.Title,
                Date = appointment.Date,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Description = appointment.Description,
                Status = appointment.Status,
                AdminComment = appointment.AdminComment
            };
        }

        private void AddAuditRecord(int appointmentId, AppointmentStatus fromStatus, AppointmentStatus toStatus, string actionBy, string comment)
        {
            _context.AppointmentAudits.Add(new AppointmentAudit
            {
                AppointmentId = appointmentId,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                ActionBy = actionBy,
                ActionAt = DateTime.UtcNow,
                Comment = comment
            });
        }
    }
}
