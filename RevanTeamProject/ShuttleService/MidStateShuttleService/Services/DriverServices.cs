using MidStateShuttleService.Models;

namespace MidStateShuttleService.Service
{
    public class DriverServices : BaseDbServices<Driver>
    {
        public DriverServices(ApplicationDbContext dbContext)
            : base(dbContext, dbContext.Drivers)
        {
        }

        // DEV NOTE: Only return active drivers for normal driver listings.
        public override IEnumerable<Driver> GetAllEntities()
        {
            return _dbSet
                .Where(driver => driver.IsActive)
                .ToList();
        }
    }
}