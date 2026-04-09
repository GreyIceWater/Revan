using MidStateShuttleService.Models;
using MidStateShuttleService.Service;

namespace MidStateShuttleService.Services
{
    public class NotificationService : BaseDbServices<Notification>
    {
        private ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext dbContext) : base(dbContext, dbContext.Notifications)
        {
            _context = dbContext;
        }

        public void SendNotification(Notification notification)
        {
            _context.Notifications.Add(notification);
            _context.SaveChanges();
        }
    }
}
