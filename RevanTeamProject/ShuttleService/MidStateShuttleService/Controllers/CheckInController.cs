using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MidStateShuttleService.Models;
using MidStateShuttleService.Service;
using MidStateShuttleService.Services;
using MidStateShuttleService.ViewModels;

namespace MidStateShuttleService.Controllers
{
    public class CheckInController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CheckInController> _logger;

        // Inject ApplicationDbContext into the controller constructor
        public CheckInController(ApplicationDbContext context, ILogger<CheckInController> logger)
        {
            _context = context; // Assign the injected ApplicationDbContext to the _context field
            _logger = logger;
        }

        // GET: CheckInController/Create
        [AllowAnonymous]
        public ActionResult CheckIn()
        {
            LocationServices ls = new LocationServices(_context);
            ViewBag.Locations = ls.GetAllEntities().Select(x => new SelectListItem { Text = x.Name, Value = x.LocationId.ToString() });

            return View();

        }

        public ActionResult EditCheckIn(int id)
        {
            var cs = new CheckInServices(_context);
            var model = cs.GetEntityById(id);

            if (model == null)
                return FailedCheckIn("Check-in Not Found");

            var ls = new LocationServices(_context);
            var locationOptions = ls.GetAllEntities()
                .Select(x => new SelectListItem { Text = x.Name, Value = x.LocationId.ToString() })
                .ToList();

            var viewModel = new CheckInViewModel
            {
                CheckInId = model.CheckInId,
                Name = model.Name,
                UtcDate = model.Date,
                Comments = model.Comments,
                FirstTime = model.FirstTime,
                LocationId = model.LocationId,
                IsActive = model.IsActive,
                LocationOptions = locationOptions
                // CentralDateTime will auto-convert from UtcDate via getter in ViewModel
            };

            return View(viewModel);
        }

        // POST: CheckInController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult CheckIn(CheckIn checkIn)
        {
            checkIn.Date = DateTime.UtcNow;
            CheckInServices cs = new CheckInServices(_context);
            checkIn.IsActive = true;
            cs.AddEntity(checkIn);

            // Increment the check-in count in the session
            int checkInCount = HttpContext.Session.GetInt32("CheckInCount") ?? 0;
            checkInCount++;
            HttpContext.Session.SetInt32("CheckInCount", checkInCount);

            // The temp data which is used to display the modal after sending a form
            HttpContext.Session.SetString("CheckInSuccess", "true");
            TempData["CheckInSuccess"] = true;

            return RedirectToAction("CheckIn");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCheckIn(CheckInViewModel model)
        {
            if (model == null)
                return FailedCheckIn("Updates to check-in could not be applied");

            var cs = new CheckInServices(_context);
            var existingCheckIn = cs.GetEntityById(model.CheckInId);

            if (existingCheckIn == null)
                return FailedCheckIn("Check-in not found");

            // Update other fields
            existingCheckIn.Name = model.Name;
            existingCheckIn.Comments = model.Comments;
            existingCheckIn.FirstTime = model.FirstTime;
            existingCheckIn.LocationId = model.LocationId;
            existingCheckIn.IsActive = true;

            // Use UtcDate from ViewModel (which is kept in sync when CentralDateTime is set)
            existingCheckIn.Date = model.UtcDate;

            cs.UpdateEntity(existingCheckIn);

            return RedirectToAction("Index", "Dashboard");
        }

        public ActionResult DeleteCheckIn(int id)
        {
            try
            {
                CheckInServices cs = new CheckInServices(_context);
                CheckIn model = cs.GetEntityById(id);

                if (model == null)
                    return FailedCheckIn("Check-in could not be found");

                model.IsActive = !model.IsActive; // Toggle IsActive value
                cs.UpdateEntity(model); // Update the entity in the database

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                // Log the exception
                LogEvents.LogSqlException(ex, (IWebHostEnvironment)_context);
                _logger.LogError(ex, "An error occurred while toggling IsActive of the check-in.");

                // Optionally add a model error for displaying an error message to the user
                ModelState.AddModelError("", "An unexpected error occurred while toggling IsActive of the check-in, please try again.");

                // Return the view with an error message or handle the error as required
                return View();
            }

        }

        public ActionResult Delete(int id)
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult FailedCheckIn(string errorMessage)
        {
            ViewBag.ErrorMessage = errorMessage;
            return View("FailedCheckIn");
        }
    }
}
