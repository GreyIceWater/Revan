using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MidStateShuttleService.Models;
using MidStateShuttleService.Service;
using MidStateShuttleService.Services;

namespace MidStateShuttleService.Controllers
{
    public class DriverController : Controller
    {
        private readonly DriverServices _driverService;
        private readonly ILogger<DriverController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;

        public DriverController(
            ApplicationDbContext context,
            DriverServices driverService,
            ILogger<DriverController> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _driverService = driverService;
            _logger = logger;
            _environment = environment;
        }

        [Authorize(Roles = "Admin")] // DEV NOTE: Admin-only list page for driver management.
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")] // DEV NOTE: Admin-only details page.
        [HttpGet]
        public IActionResult Details(int id)
        {
            Driver existingDriver = _driverService.GetEntityById(id);

            if (existingDriver == null)
                return NotFound();

            return View(existingDriver);
        }

        [Authorize(Roles = "Admin")] // DEV NOTE: Admin-only page for creating drivers.
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin")] // DEV NOTE: Admin-only form submission for creating drivers.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Driver submittedDriver)
        {
            if (!ModelState.IsValid)
                return View(submittedDriver);

            try
            {
                submittedDriver.IsActive = true;
                _driverService.AddEntity(submittedDriver);

                TempData["SuccessMessage"] = "The driver has been successfully created!";
                HttpContext.Session.SetString("DriverSuccess", "true");
                TempData["DriverSuccess"] = true;

                return RedirectToAction(nameof(Create));
            }
            catch (Exception exception)
            {
                LogEvents.LogSqlException(exception, _environment);
                _logger.LogError(exception, "An error occurred while creating driver.");

                ModelState.AddModelError("", "An unexpected error occurred, please try again.");
                return View(submittedDriver);
            }
        }

        [Authorize(Roles = "Admin")] // DEV NOTE: Admin-only page for editing drivers.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Driver existingDriver = _driverService.GetEntityById(id);

            if (existingDriver == null)
                return NotFound();

            return View(existingDriver);
        }

        [Authorize(Roles = "Admin")] // DEV NOTE: Admin-only form submission for editing drivers.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Driver submittedDriver)
        {
            if (id != submittedDriver.DriverId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(submittedDriver);

            try
            {
                submittedDriver.IsActive = true;
                _driverService.UpdateEntity(submittedDriver);

                TempData["SuccessMessage"] = "The driver has been successfully updated!";
                HttpContext.Session.SetString("DriverSuccess", "true");
                TempData["DriverSuccess"] = true;

                return RedirectToAction(nameof(Edit), new { id = submittedDriver.DriverId });
            }
            catch (Exception exception)
            {
                LogEvents.LogSqlException(exception, _environment);
                _logger.LogError(exception, "An error occurred while updating driver.");

                ModelState.AddModelError("", "An unexpected error occurred, please try again.");
                return View(submittedDriver);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult ViewAll(bool viewArchived = false)
        {
            var drivers = _context.Drivers
            .Where(d => d.IsActive == !viewArchived)
            .ToList();

            ViewData["Archives"] = viewArchived;

            return View("DriverTable", drivers);
        }

        [Authorize(Roles = "Admin")] // DEV NOTE: Admin-only action that toggles driver active status.
        [HttpPost] // DEV NOTE: Data-changing actions should use POST instead of GET.
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                Driver existingDriver = _driverService.GetEntityById(id);

                if (existingDriver == null)
                {
                    TempData["ErrorMessage"] = "Driver not found.";
                    return RedirectToAction("Index", "Dashboard");
                }

                existingDriver.IsActive = !existingDriver.IsActive;
                _driverService.UpdateEntity(existingDriver);

                return RedirectToAction("ViewAll");
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception exception)
            {
                LogEvents.LogSqlException(exception, _environment);
                _logger.LogError(exception, "An error occurred while toggling IsActive of the driver.");

                TempData["ErrorMessage"] =
                    "An unexpected error occurred while updating the driver.";

                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Unarchive(int id)
        {
            var driver = _context.Drivers.Find(id);

            if (driver == null)
                return NotFound();

            driver.IsActive = true;
            _context.SaveChanges();

            return RedirectToAction("ViewAll", new { viewArchived = true });
        }
    }
}