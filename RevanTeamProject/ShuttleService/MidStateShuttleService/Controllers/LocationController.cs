using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MidStateShuttleService.Models;
using MidStateShuttleService.Service;

namespace MidStateShuttleService.Controllers
{
    public class LocationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LocationController> _logger;

        // DEV NOTE:
        // Keep constructor injection simple and consistent with the rest of the project.
        public LocationController(ApplicationDbContext context, ILogger<LocationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: LocationController
        public ActionResult Index()
        {
            _logger.LogInformation("Location Index accessed.");
            return View();
        }

        // GET: LocationController/Create
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Create()
        {
            _logger.LogInformation("Location Create page accessed.");
            return View();
        }

        // POST: LocationController/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Location location)
        {
            _logger.LogInformation("Location Create POST received for Name: {LocationName}", location?.Name);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Location Create failed validation for Name: {LocationName}", location?.Name);
                return View(location);
            }

            try
            {
                LocationServices locationServices = new LocationServices(_context);

                // DEV NOTE:
                // New locations should always start active.
                location.IsActive = true;

                locationServices.AddEntity(location);

                _logger.LogInformation("Location created successfully for LocationId: {LocationId}", location.LocationId);

                TempData["SuccessMessage"] = "The location has been successfully created!";
                HttpContext.Session.SetString("LocationSuccess", "true");
                TempData["LocationSuccess"] = true;

                return RedirectToAction(nameof(Create));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred while creating location.");

                // DEV NOTE:
                // Show the real underlying error while debugging.
                var actualError = exception.InnerException?.Message ?? exception.Message;
                ModelState.AddModelError("", actualError);

                return View(location);
            }
        }

        // GET: LocationController/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            _logger.LogInformation("Location Edit GET requested for LocationId: {LocationId}", id);

            try
            {
                LocationServices locationServices = new LocationServices(_context);
                Location location = locationServices.GetEntityById(id);

                if (location == null)
                {
                    _logger.LogWarning("Location Edit GET failed. Location not found for LocationId: {LocationId}", id);
                    return FailedLocation("Location Not Found");
                }

                return View(location);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred while loading location {LocationId} for edit.", id);
                return FailedLocation("Location could not be loaded");
            }
        }

        // POST: LocationController/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Location model)
        {
            _logger.LogInformation("Location Edit POST received for LocationId: {LocationId}", model?.LocationId);

            if (model == null)
            {
                _logger.LogWarning("Location Edit POST failed. Model was null.");
                return FailedLocation("Updates to location could not be applied");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Location Edit POST failed validation for LocationId: {LocationId}", model.LocationId);
                return View(model);
            }

            try
            {
                LocationServices locationServices = new LocationServices(_context);
                Location existingLocation = locationServices.GetEntityById(model.LocationId);

                if (existingLocation == null)
                {
                    _logger.LogWarning("Location Edit POST failed. Location not found for LocationId: {LocationId}", model.LocationId);
                    return FailedLocation("Location Not Found");
                }

                // DEV NOTE:
                // Update only editable fields.
                // Preserve IsActive so edit does not accidentally reactivate a removed location.
                existingLocation.Name = model.Name;
                existingLocation.Address = model.Address;
                existingLocation.City = model.City;
                existingLocation.State = model.State;
                existingLocation.ZipCode = model.ZipCode;
                existingLocation.Abbreviation = model.Abbreviation;

                locationServices.UpdateEntity(existingLocation);

                _logger.LogInformation("Location updated successfully for LocationId: {LocationId}", existingLocation.LocationId);

                HttpContext.Session.SetString("LocationSuccess", "true");
                TempData["LocationSuccess"] = true;

                return RedirectToAction(nameof(Edit), new { id = existingLocation.LocationId });
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred while updating location {LocationId}.", model.LocationId);
                return FailedLocation("Updates to location could not be applied");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Driver")]
        public ActionResult ViewAll(bool viewArchived = false)
        {
            _logger.LogInformation("Location ViewAll requested. ViewArchived: {ViewArchived}", viewArchived);

            var locations = _context.Locations
            .Where(l => l.IsActive == !viewArchived)
            .ToList();

            _logger.LogInformation("Location ViewAll returned {LocationCount} records.", locations.Count);

            ViewData["Archives"] = viewArchived;

            return View("LocationTable", locations);
        }

        // POST: LocationController/DeleteLocation/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteLocation(int id)
        {
            _logger.LogInformation("Location Delete toggle requested for LocationId: {LocationId}", id);

            try
            {
                LocationServices locationServices = new LocationServices(_context);
                Location location = locationServices.GetEntityById(id);

                if (location == null)
                {
                    _logger.LogWarning("Location Delete failed. Location not found for LocationId: {LocationId}", id);
                    return FailedLocation("Location Not Found");
                }

                // DEV NOTE:
                // Soft delete only. Toggle active state instead of removing the row.
                location.IsActive = !location.IsActive;
                locationServices.UpdateEntity(location);

                _logger.LogInformation("Location IsActive toggled successfully for LocationId: {LocationId}. New IsActive: {IsActive}", id, location.IsActive);

                return RedirectToAction("ViewAll");
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred while toggling location {LocationId}.", id);
                return FailedLocation("Updates to location could not be applied");
            }
        }

        // DEV NOTE:
        // Keeping this action in place in case any old routes/views still reference it.
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Delete(int id)
        {
            _logger.LogInformation("Legacy Delete view accessed for LocationId: {LocationId}", id);
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Unarchive(int id)
        {
            _logger.LogInformation("Location Unarchive requested for LocationId: {LocationId}", id);

            var location = _context.Locations.Find(id);

            if (location == null)
            {
                _logger.LogWarning("Location Unarchive failed. Location not found for LocationId: {LocationId}", id);
                return NotFound();
            }

            location.IsActive = true;
            _context.SaveChanges();

            _logger.LogInformation("Location unarchived successfully for LocationId: {LocationId}", id);

            return RedirectToAction("ViewAll", new { viewArchived = true });
        }

        public ActionResult FailedLocation(string errorMessage)
        {
            _logger.LogWarning("FailedLocation triggered with message: {ErrorMessage}", errorMessage);

            ViewBag.ErrorMessage = errorMessage;
            return View("FailedLocation");
        }
    }
}