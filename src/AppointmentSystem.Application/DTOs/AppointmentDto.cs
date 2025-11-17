using AppointmentSystem.Domain.Entities;

namespace AppointmentSystem.Application.DTOs
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public int? RequestedById { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string BranchLocation { get; set; } = string.Empty;
        public string? Description { get; set; }
        public AppointmentStatus Status { get; set; }
        public string? AdminComment { get; set; }
    }

    public class AppointmentFilterDto
    {
        public AppointmentStatus? Status { get; set; }
        public int? BranchId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchText { get; set; }
        public string? SortBy { get; set; }
        public bool? SortDescending { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public int? RequestedById { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    public class AppointmentAuditDto
    {
        public int AppointmentId { get; set; }
        public AppointmentStatus FromStatus { get; set; }
        public AppointmentStatus ToStatus { get; set; }
        public string ActionBy { get; set; } = string.Empty;
        public DateTime ActionAt { get; set; }
        public string? Comment { get; set; }
    }
}
