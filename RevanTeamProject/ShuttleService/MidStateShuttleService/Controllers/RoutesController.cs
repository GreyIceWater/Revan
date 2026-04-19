using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MidStateShuttleService.Enums;
using MidStateShuttleService.Helpers;
using MidStateShuttleService.Models;
using MidStateShuttleService.Service;

namespace MidStateShuttleService.Controllers
{
    public class RoutesController : Controller
    {
        private readonly ILogger<RoutesController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        // Inject ApplicationDbContext into the controller constructor
        public RoutesController(
            ApplicationDbContext context,
            ILogger<RoutesController> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;

            _logger.LogInformation("RoutesController initialized.");
        }

        // GET: RoutesController
        public ActionResult Index()
        {
            _logger.LogInformation("Routes Index action called.");
            return View();
        }

        // GET: RoutesController/Details/5
        public ActionResult Details(int id)
        {
            _logger.LogInformation("Routes Details action called for RouteId: {RouteId}", id);
            return View();
        }

        // GET: RoutesController/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            _logger.LogInformation("Routes Create GET action called.");
            LoadRouteDropdowns();
            return View();
        }

        // POST: RoutesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(Routes route)
        {
            _logger.LogInformation("Routes Create POST action called.");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Routes Create POST failed ModelState validation.");
                LoadRouteDropdowns();
                return View(route);
            }

            try
            {
                RouteServices rs = new RouteServices(_context);
                route.IsActive = true;
                rs.AddEntity(route);

                _logger.LogInformation("Route created successfully.");

                HttpContext.Session.SetString("RouteSuccess", "true");
                TempData["RouteSuccess"] = true;

                return RedirectToAction(nameof(Create));
            }
            catch (Exception ex)
            {
                LogEvents.LogSqlException(ex, _environment);
                _logger.LogError(ex, "An error occurred while creating the route.");
                LoadRouteDropdowns();
                ModelState.AddModelError("", "An unexpected error occurred while creating the route.");
                return View(route);
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult CreateFromRide(int rideId)
        {
            _logger.LogInformation("CreateFromRide action called for RideId: {RideId}", rideId);

            Ride ride = _context.Rides.Where(r => r.RideId == rideId).FirstOrDefault();
            RequestDay rDay = _context.RequestDays.Where(d => d.RequestDayId == ride.RequestDayId).FirstOrDefault();

            WeekDay dayOfWeek = rDay.WeekDay;

            //fallback incase of null
            if (ride == null)
            {
                _logger.LogWarning("CreateFromRide could not find RideId: {RideId}", rideId);
                RedirectToAction(nameof(Create));
            }

            Routes route = new Routes();

            route.IsActive = true;
            route.PickUpTime = ride.DropOffTime.Value.ToTimeSpan();
            route.DropOffTime = route.PickUpTime.Value.Add(TimeSpan.FromMinutes(30));
            route.DropOffLocationID = ride.DropOffLocationID;
            route.PickUpLocationID = ride.PickUpLocationID;
            route.DayOfWeek = dayOfWeek;

            try
            {
                RouteServices rs = new RouteServices(_context);
                route.IsActive = true;
                rs.AddEntity(route);

                _logger.LogInformation("Route created successfully from RideId: {RideId}", rideId);

                HttpContext.Session.SetString("RouteSuccess", "true");
                TempData["RouteSuccess"] = true;

                return RedirectToAction(nameof(Create));
            }
            catch (Exception ex)
            {
                LogEvents.LogSqlException(ex, _environment);
                _logger.LogError(ex, "An error occurred while creating the route.");
                LoadRouteDropdowns();
                ModelState.AddModelError("", "An unexpected error occurred while creating the route.");
                return View(route);
            }
        }

        // GET: RoutesController/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            _logger.LogInformation("Routes Edit GET action called for RouteId: {RouteId}", id);

            var route = _context.Routes.Find(id);

            if (route == null)
            {
                _logger.LogWarning("Routes Edit GET could not find RouteId: {RouteId}", id);
                return NotFound();
            }

            LoadRouteDropdowns();
            return View(route);
        }

        // POST: RoutesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id, Routes updatedRoute)
        {
            _logger.LogInformation("Routes Edit POST action called for RouteId: {RouteId}", id);

            if (id != updatedRoute.RouteID)
            {
                _logger.LogWarning("Routes Edit POST received mismatched RouteId. UrlId: {UrlId}, ModelId: {ModelId}", id, updatedRoute.RouteID);
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Routes Edit POST failed ModelState validation for RouteId: {RouteId}", id);
                LoadRouteDropdowns();
                return View(updatedRoute);
            }

            try
            {
                updatedRoute.IsActive = true;
                _context.Update(updatedRoute);
                _context.SaveChanges();

                _logger.LogInformation("Route updated successfully for RouteId: {RouteId}", updatedRoute.RouteID);

                HttpContext.Session.SetString("RouteSuccess", "true");
                TempData["RouteSuccess"] = true;
                TempData["SuccessMessage"] = "The route has been successfully updated!";

                return RedirectToAction(nameof(Edit), new { id = updatedRoute.RouteID });
            }
            catch (Exception ex)
            {
                LogEvents.LogSqlException(ex, _environment);
                _logger.LogError(ex, "An error occurred while updating the route.");
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Driver")]
        public ActionResult ViewAll(bool viewArchived = false)
        {
            _logger.LogInformation("Routes ViewAll action called. ViewArchived: {ViewArchived}", viewArchived);

            var routes = _context.Routes
                .Include(r => r.PickUpLocation)
                .Include(r => r.DropOffLocation)
                .Where(r => r.IsActive == !viewArchived)
                .ToList();

            ViewData["Archives"] = viewArchived;

            _logger.LogInformation("Routes ViewAll returning {RouteCount} routes.", routes.Count);
            return View("RouteTable", routes);
        }

        [HttpGet]
        public ActionResult ViewScheduleTable()
        {
            _logger.LogInformation("Routes ViewScheduleTable action called.");

            var routes = _context.Routes
                .Include(r => r.PickUpLocation)
                .Include(r => r.DropOffLocation)
                .Where(r => r.IsActive)
                .ToList();

            _logger.LogInformation("Routes ViewScheduleTable returning {RouteCount} routes.", routes.Count);
            return View("ScheduleTable", routes);
        }

        // GET: RoutesController/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            _logger.LogInformation("Routes Delete GET action called for RouteId: {RouteId}", id);

            try
            {
                var route = _context.Routes.Find(id);

                if (route != null)
                {
                    route.IsActive = !route.IsActive; // Toggle IsActive from true to false or false to true
                    _context.SaveChanges();

                    _logger.LogInformation("Route IsActive toggled successfully for RouteId: {RouteId}", id);
                }
                else
                {
                    // Handle the case where the route with the specified id is not found
                    _logger.LogWarning("Routes Delete GET could not find RouteId: {RouteId}", id);
                    ModelState.AddModelError("", "Route not found.");
                    return View();
                }

                return RedirectToAction("ViewAll");
            }
            catch (Exception ex)
            {
                // Log the exception
                LogEvents.LogSqlException(ex, (IWebHostEnvironment)_context);
                _logger.LogError(ex, "An error occurred while toggling IsActive of the route.");

                // Optionally add a model error for displaying an error message to the user
                ModelState.AddModelError("", "An unexpected error occurred while toggling IsActive of the route, please try again.");

                // Return the view with an error message
                return View();
            }

            return RedirectToAction("Index", "Dashboard");
        }

        // POST: RoutesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            _logger.LogInformation("Routes Delete POST action called for RouteId: {RouteId}", id);

            try
            {
                var route = _context.Routes.Find(id);

                if (route == null)
                {
                    _logger.LogWarning("Routes Delete POST could not find RouteId: {RouteId}", id);
                    TempData["ErrorMessage"] = "Route not found.";
                    return RedirectToAction("ViewAll");
                }

                route.IsActive = !route.IsActive;
                _context.SaveChanges();

                _logger.LogInformation("Route IsActive toggled successfully for RouteId: {RouteId}", id);
                return RedirectToAction("ViewAll");
            }
            catch (Exception ex)
            {
                LogEvents.LogSqlException(ex, _environment);
                _logger.LogError(ex, "An error occurred while toggling IsActive of the route.");
                TempData["ErrorMessage"] = "An unexpected error occurred while updating the route.";
                return RedirectToAction("ViewAll");
            }
            catch
            {
                _logger.LogError("An unknown error occurred in Routes Delete POST for RouteId: {RouteId}", id);
                return View();
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Unarchive(int id)
        {
            _logger.LogInformation("Routes Unarchive action called for RouteId: {RouteId}", id);

            var route = _context.Routes.Find(id);

            if (route == null)
            {
                _logger.LogWarning("Routes Unarchive could not find RouteId: {RouteId}", id);
                return NotFound();
            }

            route.IsActive = true;
            _context.SaveChanges();

            _logger.LogInformation("Route unarchived successfully for RouteId: {RouteId}", id);
            return RedirectToAction("ViewAll", new { viewArchived = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetRoutes(int pickupId, int dropoffId, int dayOfWeek)
        {
            _logger.LogInformation("Routes GetRoutes action called. PickupId: {PickupId}, DropoffId: {DropoffId}, DayOfWeek: {DayOfWeek}", pickupId, dropoffId, dayOfWeek);

            WeekDay weekDay = (WeekDay)dayOfWeek;

            var routes = await _context.Routes
                .Where(r =>
                    r.PickUpLocationID == pickupId &&
                    r.DropOffLocationID == dropoffId &&
                    r.DayOfWeek == weekDay &&
                    r.IsActive == true)
                .ToListAsync();

            var result = routes.Select(r => new
            {
                id = r.RouteID,
                pickupTime = FormatTime(r.PickUpTime),
                dropoffTime = FormatTime(r.DropOffTime)
            });

            _logger.LogInformation("Routes GetRoutes returning matching routes.");
            return Json(result);
        }

        private static string FormatTime(TimeSpan? time)
        {
            if (time == null)
                return "";

            return DateTime.Today.Add(time.Value).ToString("h:mm tt");
        }

        // Helper method used by Create/Edit views to populate dropdown lists
        private void LoadRouteDropdowns()
        {
            _logger.LogInformation("LoadRouteDropdowns called.");

            // Load all ACTIVE locations for the pickup/drop-off dropdowns
            LocationServices ls = new LocationServices(_context);
            ViewBag.Locations = ls.GetAllEntities()
                .Where(location => location.IsActive)
                .Select(location => new SelectListItem
                {
                    Text = location.Name,                    // Location name shown to user
                    Value = location.LocationId.ToString()   // Location ID submitted with form
                });

            // Load drivers so a route can be assigned to one
            DriverServices ds = new DriverServices(_context);
            ViewBag.Drivers = ds.GetAllEntities()
                .Select(driver => new SelectListItem
                {
                    Text = driver.Name,                 // Driver name shown in dropdown
                    Value = driver.DriverId.ToString()  // Driver ID submitted
                });

            // Load buses/shuttles for route assignment
            BusServices bs = new BusServices(_context);
            ViewBag.Buses = bs.GetAllEntities()
                .Select(bus => new SelectListItem
                {
                    Text = "Shuttle: " + bus.BusNo,     // Label shown in dropdown
                    Value = bus.BusId.ToString()        // Bus ID submitted
                });

            _logger.LogInformation("LoadRouteDropdowns completed.");
        }
    }
}