using AppointmentSystem.Application.DTOs;

namespace AppointmentSystem.Web.Helpers
{
    public static class QueryStringHelper
    {
        public static string BuildQueryString(AppointmentFilterDto filter)
        {
            var queryParams = new List<string>();

            if (filter.Status.HasValue)
                queryParams.Add($"Status={(int)filter.Status.Value}");

            if (filter.BranchId.HasValue)
                queryParams.Add($"BranchId={filter.BranchId.Value}");

            if (filter.StartDate.HasValue)
                queryParams.Add($"StartDate={filter.StartDate.Value:yyyy-MM-dd}");

            if (filter.EndDate.HasValue)
                queryParams.Add($"EndDate={filter.EndDate.Value:yyyy-MM-dd}");

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
                queryParams.Add($"SearchText={Uri.EscapeDataString(filter.SearchText)}");

            if (!string.IsNullOrWhiteSpace(filter.SortBy))
                queryParams.Add($"SortBy={Uri.EscapeDataString(filter.SortBy)}");

            if (filter.SortDescending.HasValue)
                queryParams.Add($"SortDescending={filter.SortDescending.Value}");

            if (filter.PageNumber.HasValue)
                queryParams.Add($"PageNumber={filter.PageNumber.Value}");

            if (filter.PageSize.HasValue)
                queryParams.Add($"PageSize={filter.PageSize.Value}");

            if (filter.RequestedById.HasValue)
                queryParams.Add($"RequestedById={filter.RequestedById.Value}");

            return queryParams.Any() ? "?" + string.Join("&", queryParams) : string.Empty;
        }
    }
}

