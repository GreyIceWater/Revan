using Microsoft.AspNetCore.Mvc.Rendering;
using MidStateShuttleService.Services;

namespace MidStateShuttleService.ViewModels
{
    public class CheckInViewModel
    {
        public int CheckInId { get; set; }

        public string? Name { get; set; }

        public TimeOnly? CheckInTime { get; set; }

        public DateTime UtcDate { get; set; }

        public DateTime CentralDate => TimeService.ConvertUtcToCentral(UtcDate);

        public string? Comments { get; set; }

        public bool FirstTime { get; set; }

        public int LocationId { get; set; }

        public bool IsActive { get; set; }

        public IEnumerable<SelectListItem> LocationOptions { get; set; } = new List<SelectListItem>();

        // New combined property for binding to datetime-local input in the view
        public DateTime CentralDateTime
        {
            get => TimeService.ConvertUtcToCentral(UtcDate);
            set
            {
                // When setting, split into UtcDate and CheckInTime to keep old properties consistent
                UtcDate = TimeService.ConvertCentralToUtc(value);
                CheckInTime = TimeOnly.FromDateTime(value);
            }
        }
    }
}
