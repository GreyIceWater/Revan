using Microsoft.AspNetCore.Mvc.Rendering;
using MidStateShuttleService.Models;
using MidStateShuttleService.Service;

namespace MidStateShuttleService.Service
{
    public class LocationServices : BaseDbServices<Location>
    {
        public LocationServices(ApplicationDbContext dbContext) : base(dbContext, dbContext.Locations)
        {

        }

        public IEnumerable<SelectListItem> GetLocationNames()
        {
            var locations = new List<SelectListItem>();

            foreach (Location l in GetAllEntities().Where(l => l.IsActive))
            {
                locations.Add(new SelectListItem
                {
                    Value = l.LocationId.ToString(),
                    Text = l.Name.ToString()
                });
            }

            return locations;
        }

        public string getLocationNameById(int id)
        {
            return GetEntityById(id).Name;
        }
    }
}
