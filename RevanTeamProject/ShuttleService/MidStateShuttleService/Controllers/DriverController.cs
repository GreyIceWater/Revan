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
            _logger.LogInformation("Driver index page accessed.");
            return View();
        }

        [Authorize(Roles = "Admin")] // DEV NOTE: Admin-only details page.
        [HttpGet]
        public IActionResult Details(int id)
        {
            _logger.LogInformation("Driver details requested for DriverId: {DriverId}", id);

            Driver existingDriver = _driverService.GetEntityById(id);

            if (existingDriver == null)
            {
                _logger.LogWarning("Driver details request failed. Driver not found for DriverId: {DriverId}", id);
                return NotFound();
            }

            return View(existingDriver);
        }

        [Authorize(Roles = "Admin")] // DEV NOTE: Admin-only page for creating drivers.
        [HttpGet]
        public IActionResult Create()
        {
            _logger.LogInformation("Driver create page accessed.");
            return View();
        }

        [Authorize(Roles = "Admin")] // DEV NOTE: Admin-only form submission for creating drivers.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Driver submittedDriver)
        {
            _logger.LogInformation("Driver create submitted for Name: {DriverName}", submittedDriver?.Name);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Driver create failed validation for Name: {DriverName}", submittedDriver?.Name);
                return View(submittedDriver);
            }

            try
            {
                submittedDriver.IsActive = true;
                _driverService.AddEntity(submittedDriver);

                _logger.LogInformation("Driver created successfully for DriverId: {DriverId}", submittedDriver.DriverId);

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
            _logger.LogInformation("Driver edit page requested for DriverId: {DriverId}", id);

            Driver existingDriver = _driverService.GetEntityById(id);

            if (existingDriver == null)
            {
                _logger.LogWarning("Driver edit page failed. Driver not found for DriverId: {DriverId}", id);
                return NotFound();
            }

            return View(existingDriver);
        }

        [Authorize(Roles = "Admin")] // DEV NOTE: Admin-only form submission for editing drivers.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Driver submittedDriver)
        {
            _logger.LogInformation("Driver edit submitted for DriverId: {DriverId}", id);

            if (id != submittedDriver.DriverId)
            {
                _logger.LogWarning("Driver edit failed due to id mismatch. Route id: {RouteId}, Model id: {ModelId}", id, submittedDriver.DriverId);
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Driver edit failed validation for DriverId: {DriverId}", submittedDriver.DriverId);
                return View(submittedDriver);
            }

            try
            {
                submittedDriver.IsActive = true;
                _driverService.UpdateEntity(submittedDriver);

                _logger.LogInformation("Driver updated successfully for DriverId: {DriverId}", submittedDriver.DriverId);

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
            _logger.LogInformation("Driver ViewAll requested. ViewArchived: {ViewArchived}", viewArchived);

            var drivers = _context.Drivers
            .Where(d => d.IsActive == !viewArchived)
            .ToList();

            ViewData["Archives"] = viewArchived;

            _logger.LogInformation("Driver ViewAll returned {DriverCount} records.", drivers.Count);

            return View("DriverTable", drivers);
        }

        [Authorize(Roles = "Admin")] // DEV NOTE: Admin-only action that toggles driver active status.
        [HttpPost] // DEV NOTE: Data-changing actions should use POST instead of GET.
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _logger.LogInformation("Driver delete toggle requested for DriverId: {DriverId}", id);

            try
            {
                Driver existingDriver = _driverService.GetEntityById(id);

                if (existingDriver == null)
                {
                    _logger.LogWarning("Driver delete toggle failed. Driver not found for DriverId: {DriverId}", id);
                    TempData["ErrorMessage"] = "Driver not found.";
                    return RedirectToAction("Index", "Dashboard");
                }

                existingDriver.IsActive = !existingDriver.IsActive;
                _driverService.UpdateEntity(existingDriver);

                _logger.LogInformation("Driver IsActive toggled successfully for DriverId: {DriverId}. New IsActive: {IsActive}", id, existingDriver.IsActive);

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
            _logger.LogInformation("Driver unarchive requested for DriverId: {DriverId}", id);

            var driver = _context.Drivers.Find(id);

            if (driver == null)
            {
                _logger.LogWarning("Driver unarchive failed. Driver not found for DriverId: {DriverId}", id);
                return NotFound();
            }

            driver.IsActive = true;
            _context.SaveChanges();

            _logger.LogInformation("Driver unarchived successfully for DriverId: {DriverId}", id);

            return RedirectToAction("ViewAll", new { viewArchived = true });
        }
    }
}