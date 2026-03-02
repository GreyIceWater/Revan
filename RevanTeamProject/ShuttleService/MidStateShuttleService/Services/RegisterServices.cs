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
        

        public List<RegisterModel> GetEmailsByRoute(string routeId)
        {
            var mailingList = _dbSet.Where(x => x.SelectedRouteDetail == routeId || x.ReturnSelectedRouteDetail == routeId).ToList();

            return mailingList;
        }

        /// <summary>
        /// Gets the list of view models of all the requests/registrations
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RegistrationViewModel> GetViewModels()
        {
            var registrations = _context.RegisterModels
                .Include(r => r.User)
                .Include(r => r.DaySchedules)
                    .ThenInclude(d => d.Rides)
                    .Where(r => !r.IsArchived)
                .Select(r => new RegistrationViewModel
                {
                    RegistrationId = r.RegistrationId,

                    UserName = r.Name != null ? r.Name : "Unknown",
                    StudentId = r.StudentId != null ? r.StudentId : "No ID",

                    Term = r.Term ?? SchoolTerm.Fall,

                    RequestDays = r.isCustom
                        ? new List<RequestDayViewModel>()
                        : r.DaySchedules.Select(d => new RequestDayViewModel
                        {
                            WeekDay = d.WeekDay,

                            Rides = d.Rides.Select(ride => new RideViewModel
                            {
                                PickUpLocation = ride.PickUpLocationID.ToString(),
                                DropOffLocation = ride.DropOffLocationID.ToString(),
                                DropOffTime = ride.DropOffTime.ToString("hh\\:mm")
                            }).ToList()

                        }).ToList(),

                    dateCreated = r.InsertDateTime ?? DateTime.MinValue,

                    IsCustom = r.isCustom
                });

            return registrations;
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
