using Microsoft.EntityFrameworkCore;
using MidStateShuttleService.Models;
using MidStateShuttleService.ViewModels;
using System.Collections;

namespace MidStateShuttleService.Service
{
    public class RegisterServices : BaseDbServices<RegisterModel>
    {
        private readonly ApplicationDbContext _context;

        public RegisterServices(ApplicationDbContext dbContext) : base(dbContext, dbContext.RegisterModels)
        {
            _context = dbContext;
        }

        // Retrieve registrations with matching pickup and drop-off locations


        public List<RegisterModel> GetEmailsByRoute(int routeId)
        {
            var mailingList = _dbSet
                .Where(r => r.DaySchedules
                    .Any(dr => dr.Rides
                        .Any(ride => ride.Route != null && ride.RouteId == routeId)))
                .ToList();

            return mailingList;
        }

        public int GetRegistrationCount(string range)
        {
            var today = DateTime.Today;
            DateTime start;
            DateTime end;

            switch (range?.ToLower())
            {
                case "day":
                    start = today;
                    end = today.AddDays(1);
                    break;

                case "week":
                    int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                    start = today.AddDays(-diff);
                    end = start.AddDays(7);
                    break;

                default:
                    return 0;
            }

            return _context.RegisterModels
                .Where(r => r.InsertDateTime.HasValue &&
                            r.InsertDateTime.Value >= start &&
                            r.InsertDateTime.Value < end)
                .Count();
        }
    }
}
