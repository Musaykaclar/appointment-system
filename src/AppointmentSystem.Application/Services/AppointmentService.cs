using AppointmentSystem.Application.DTOs;
using AppointmentSystem.Domain.Entities;

namespace AppointmentSystem.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly List<AppointmentDto> _appointments = new();
        private readonly List<AppointmentAuditDto> _audits = new();
        private int _nextId = 1;

        public Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(AppointmentFilterDto filter)
        {
            var query = _appointments.AsQueryable();

            // Filtreleme
            if (filter.Status.HasValue)
            {
                query = query.Where(a => a.Status == filter.Status.Value);
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

            // Sıralama
            query = filter.SortBy?.ToLower() switch
            {
                "date" => filter.SortDescending 
                    ? query.OrderByDescending(a => a.Date) 
                    : query.OrderBy(a => a.Date),
                "status" => filter.SortDescending 
                    ? query.OrderByDescending(a => a.Status) 
                    : query.OrderBy(a => a.Status),
                _ => query.OrderByDescending(a => a.Date)
            };

            var totalCount = query.Count();
            var items = query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            return Task.FromResult(new PagedResult<AppointmentDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            });
        }

        public Task<PagedResult<AppointmentDto>> GetPendingAppointmentsAsync(AppointmentFilterDto filter)
        {
            filter.Status = AppointmentStatus.Pending;
            return GetAppointmentsAsync(filter);
        }

        public Task<AppointmentDto?> GetAppointmentByIdAsync(int id)
        {
            return Task.FromResult(_appointments.FirstOrDefault(a => a.Id == id));
        }

        public Task<List<AppointmentAuditDto>> GetAppointmentAuditsAsync(int appointmentId)
        {
            var audits = _audits
                .Where(a => a.AppointmentId == appointmentId)
                .OrderByDescending(a => a.ActionAt)
                .ToList();
            return Task.FromResult(audits);
        }

        public Task CreateAppointmentAsync(AppointmentDto dto)
        {
            dto.Id = _nextId++;
            dto.Status = AppointmentStatus.Pending; // Gönder butonu ile Pending durumuna çekilir
            _appointments.Add(dto);

            // Audit kaydı
            _audits.Add(new AppointmentAuditDto
            {
                AppointmentId = dto.Id,
                FromStatus = AppointmentStatus.Draft,
                ToStatus = AppointmentStatus.Pending,
                ActionBy = dto.RequestedBy,
                ActionAt = DateTime.UtcNow,
                Comment = "Randevu talebi gönderildi"
            });

            return Task.CompletedTask;
        }

        public Task UpdateAppointmentAsync(AppointmentDto dto)
        {
            var existing = _appointments.FirstOrDefault(a => a.Id == dto.Id);
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

                if (oldStatus != dto.Status)
                {
                    _audits.Add(new AppointmentAuditDto
                    {
                        AppointmentId = dto.Id,
                        FromStatus = oldStatus,
                        ToStatus = dto.Status,
                        ActionBy = dto.RequestedBy,
                        ActionAt = DateTime.UtcNow,
                        Comment = "Randevu güncellendi"
                    });
                }
            }
            return Task.CompletedTask;
        }

        public Task ApproveAppointmentAsync(int id, string adminUser)
        {
            var appointment = _appointments.FirstOrDefault(a => a.Id == id);
            if (appointment != null && appointment.Status == AppointmentStatus.Pending)
            {
                var oldStatus = appointment.Status;
                appointment.Status = AppointmentStatus.Approved;

                _audits.Add(new AppointmentAuditDto
                {
                    AppointmentId = id,
                    FromStatus = oldStatus,
                    ToStatus = AppointmentStatus.Approved,
                    ActionBy = adminUser,
                    ActionAt = DateTime.UtcNow,
                    Comment = "Randevu onaylandı"
                });
            }
            return Task.CompletedTask;
        }

        public Task RejectAppointmentAsync(int id, string adminUser, string comment)
        {
            var appointment = _appointments.FirstOrDefault(a => a.Id == id);
            if (appointment != null && appointment.Status == AppointmentStatus.Pending)
            {
                var oldStatus = appointment.Status;
                appointment.Status = AppointmentStatus.Rejected;
                appointment.AdminComment = comment;

                _audits.Add(new AppointmentAuditDto
                {
                    AppointmentId = id,
                    FromStatus = oldStatus,
                    ToStatus = AppointmentStatus.Rejected,
                    ActionBy = adminUser,
                    ActionAt = DateTime.UtcNow,
                    Comment = comment
                });
            }
            return Task.CompletedTask;
        }
    }
}
