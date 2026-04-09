using MidStateShuttleService.Models;

namespace MidStateShuttleService.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalMonthlyCheckins { get; set; }

        public int PastWeekRegistrations { get; set; }

        public int TotalRequests { get; set; }

        public List<Message> Messages { get; set; }

        public List<Notification> Notifications { get; set; }
    }
}
