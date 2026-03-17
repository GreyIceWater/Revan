using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MidStateShuttleService.Enums;
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
        }

        // GET: RoutesController
        public ActionResult Index()
        {
            return View();
        }

        // GET: RoutesController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: RoutesController/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            LoadRouteDropdowns();
            return View();
        }

        // POST: RoutesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(Routes route)
        {
            if (!ModelState.IsValid)
            {
                LoadRouteDropdowns();
                return View(route);
            }

            try
            {
                RouteServices rs = new RouteServices(_context);
                route.IsActive = true;
                rs.AddEntity(route);

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
            var route = _context.Routes.Find(id);

            if (route == null)
            {
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
            if (id != updatedRoute.RouteID)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                LoadRouteDropdowns();
                return View(updatedRoute);
            }

            try
            {
                updatedRoute.IsActive = true;
                _context.Update(updatedRoute);
                _context.SaveChanges();

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
        public ActionResult ViewAll()
        {
            var routes = _context.Routes
                .Include(r => r.PickUpLocation)
                .Include(r => r.DropOffLocation)
                .Include(r => r.Driver)
                .ToList();

            return View("RouteTable", routes);
        }

        // GET: RoutesController/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            try
            {
                var route = _context.Routes.Find(id);

                if (route != null)
                {
                    route.IsActive = !route.IsActive; // Toggle IsActive from true to false or false to true
                    _context.SaveChanges();
                }
                else
                {
                    // Handle the case where the route with the specified id is not found
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
            try
            {
                var route = _context.Routes.Find(id);

                if (route == null)
                {
                    TempData["ErrorMessage"] = "Route not found.";
                    return RedirectToAction("ViewAll");
                }

                route.IsActive = !route.IsActive;
                _context.SaveChanges();

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
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRoutes(int pickupId, int dropoffId, int dayOfWeek)
        {
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
        }
    }
}