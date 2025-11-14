namespace AppointmentSystem.Domain.Entities
{
    public class AppointmentAudit
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; } = null!;
        public AppointmentStatus FromStatus { get; set; }
        public AppointmentStatus ToStatus { get; set; }
        public string ActionBy { get; set; } = string.Empty;
        public DateTime ActionAt { get; set; } = DateTime.UtcNow;
        public string? Comment { get; set; }
    }
}
