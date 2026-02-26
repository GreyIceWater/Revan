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

                    UserName = r.User != null ? r.User.FirstName : "Unknown",
                    StudentId = r.User != null ? r.User.StudentId : "N/A",

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
    }
}
