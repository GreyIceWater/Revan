using MidStateShuttleService.Enums;
using MidStateShuttleService.Models;

namespace MidStateShuttleService.ViewModels
{
    public class RegistrationViewModel
    {
        public int RegistrationId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string StudentId { get; set; } = string.Empty;

        public SchoolTerm Term { get; set; }

        public LengthOfRequest Length { get; set; }

        public List<RequestDayViewModel> RequestDays { get; set; } = new();

        public DateTime dateCreated { get; set; }

        public bool IsCustom { get; set; }
    }

    public class RequestDayViewModel
    {
        public WeekDay WeekDay { get; set; }

        public List<RideViewModel> Rides { get; set; } = new();
    }

    public class RideViewModel
    {
        public string PickUpLocation { get; set; } = string.Empty;

        public string DropOffLocation { get; set; } = string.Empty;

        public string DropOffTime { get; set; } = string.Empty;
    }
}