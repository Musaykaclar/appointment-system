using AppointmentSystem.Domain.Entities;
using MudBlazor;

namespace AppointmentSystem.Web.Helpers
{
    public static class AppointmentStatusHelper
    {
        public static Color GetStatusColor(AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Draft => Color.Default,
                AppointmentStatus.Pending => Color.Warning,
                AppointmentStatus.Approved => Color.Success,
                AppointmentStatus.Rejected => Color.Error,
                _ => Color.Default
            };
        }

        public static string GetStatusText(AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Draft => "Taslak",
                AppointmentStatus.Pending => "Beklemede",
                AppointmentStatus.Approved => "Onaylandı",
                AppointmentStatus.Rejected => "Reddedildi",
                _ => status.ToString()
            };
        }

        public static Color GetStatusChipColor(AppointmentStatus? status)
        {
            return status switch
            {
                AppointmentStatus.Pending => Color.Warning,
                AppointmentStatus.Approved => Color.Success,
                AppointmentStatus.Rejected => Color.Error,
                _ => Color.Info
            };
        }

        public static string GetStatusChipText(AppointmentStatus? status)
        {
            return status switch
            {
                AppointmentStatus.Pending => "Bekleyen Talep",
                AppointmentStatus.Approved => "Onaylanan Randevu",
                AppointmentStatus.Rejected => "Reddedilen Randevu",
                _ => "Toplam Randevu"
            };
        }
    }
}

