namespace AppointmentSystem.Domain.Entities
{
    public class Appointment
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;
        public string RequestedBy { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Description { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Draft;
        public string? AdminComment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public List<AppointmentAudit> Audits { get; set; } = new();
    }
}
