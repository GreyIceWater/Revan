using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MidStateShuttleService.Models;
using MidStateShuttleService.Service;
using MidStateShuttleService.Services;
using MidStateShuttleService.ViewModels;

namespace MidStateShuttleService.Controllers
{
    public class CheckInController : Controller
    {
        private readonly CheckInServices _checkInService;
        private readonly LocationServices _locationService;
        private readonly ILogger<CheckInController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;

        public CheckInController(
            ApplicationDbContext context,
            CheckInServices checkInService,
            LocationServices locationService,
            ILogger<CheckInController> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _checkInService = checkInService;
            _locationService = locationService;
            _logger = logger;
            _environment = environment;
        }

        [AllowAnonymous] // DEV NOTE: Public endpoint used by riders to access the check-in form.
        [HttpGet]
        public IActionResult CheckIn()
        {
            ViewBag.Locations = GetLocationOptions(); // DEV NOTE: Dropdown population logic centralized below.
            return View();
        }

        [HttpPost]
        [AllowAnonymous] // DEV NOTE: Riders submit check-ins without authentication.
        [ValidateAntiForgeryToken]
        public IActionResult CheckIn(CheckIn submittedCheckIn)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Locations = GetLocationOptions();
                return View(submittedCheckIn);
            }

            submittedCheckIn.Date = DateTime.UtcNow;
            submittedCheckIn.IsActive = true;

            // DEV NOTE: Database operations should remain inside service classes.
            _checkInService.AddEntity(submittedCheckIn);

            // DEV NOTE: Session tracking logic could be moved to a SessionTrackingService if reused elsewhere.
            int currentCheckInCount = HttpContext.Session.GetInt32("CheckInCount") ?? 0;
            HttpContext.Session.SetInt32("CheckInCount", currentCheckInCount + 1);

            // DEV NOTE: Used to trigger a success modal after redirect.
            HttpContext.Session.SetString("CheckInSuccess", "true");
            TempData["CheckInSuccess"] = true;

            return RedirectToAction(nameof(CheckIn));
        }

        [Authorize(Roles = "Admin")] // DEV NOTE: Only administrators can edit existing check-ins.
        [HttpGet]
        public IActionResult EditCheckIn(int checkInId)
        {
            CheckIn existingCheckIn = _checkInService.GetEntityById(checkInId);

            if (existingCheckIn == null)
                return FailedCheckIn("Check-in not found.");

            var viewModel = new CheckInViewModel
            {
                CheckInId = existingCheckIn.CheckInId,
                Name = existingCheckIn.Name,
                UtcDate = existingCheckIn.Date,
                Comments = existingCheckIn.Comments,
                FirstTime = existingCheckIn.FirstTime,
                LocationId = existingCheckIn.LocationId,
                IsActive = existingCheckIn.IsActive,
                StudentId = existingCheckIn.StudentId,
                DropOffLocationId = existingCheckIn.DropOffLocationId,
                LocationOptions = GetLocationOptions()
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCheckIn(CheckInViewModel submittedModel)
        {
            if (!ModelState.IsValid)
            {
                submittedModel.LocationOptions = GetLocationOptions();
                return View(submittedModel);
            }

            CheckIn existingCheckIn = _checkInService.GetEntityById(submittedModel.CheckInId);

            if (existingCheckIn == null)
                return FailedCheckIn("Check-in not found.");

            existingCheckIn.Name = submittedModel.Name;
            existingCheckIn.Comments = submittedModel.Comments;
            existingCheckIn.FirstTime = submittedModel.FirstTime;
            existingCheckIn.LocationId = submittedModel.LocationId;
            existingCheckIn.IsActive = true;

            // DEV NOTE: ViewModel stores UTC internally to keep DB timestamps consistent.
            existingCheckIn.Date = submittedModel.UtcDate;

            _checkInService.UpdateEntity(existingCheckIn);

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Driver")]
        public ActionResult ViewAll()
        {
            var checkins = new CheckInServices(_context).GetAllEntities();

            return View("CheckInTable", checkins);
        }

        [Authorize(Roles = "Admin")] // DEV NOTE: Admin-only operation that toggles check-in active state.
        [HttpPost] // DEV NOTE: Data modification endpoints should use POST instead of GET.
        [ValidateAntiForgeryToken]
        public IActionResult ToggleCheckInActive(int checkInId)
        {
            try
            {
                CheckIn existingCheckIn = _checkInService.GetEntityById(checkInId);

                if (existingCheckIn == null)
                    return FailedCheckIn("Check-in could not be found.");

                existingCheckIn.IsActive = !existingCheckIn.IsActive;

                _checkInService.UpdateEntity(existingCheckIn);

                return RedirectToAction("ViewAll");
            }
            catch (Exception exception)
            {
                // DEV NOTE: Logging and SQL exception capture should remain centralized.
                LogEvents.LogSqlException(exception, _environment);

                _logger.LogError(exception,
                    "Error toggling check-in active status for CheckInId {CheckInId}",
                    checkInId);

                TempData["ErrorMessage"] =
                    "An unexpected error occurred while updating the check-in.";

                return RedirectToAction("Index", "Dashboard");
            }
        }

        [AllowAnonymous] // DEV NOTE: Shared error view for failed check-in operations.
        [HttpGet]
        public IActionResult FailedCheckIn(string errorMessage)
        {
            ViewBag.ErrorMessage = errorMessage;
            return View("FailedCheckIn");
        }

        // DEV NOTE:
        // Helper method used to build location dropdown options.
        // If multiple controllers require this logic, it should be moved
        // into LocationServices as something like GetLocationSelectList().
        private List<SelectListItem> GetLocationOptions()
        {
            var locations = _locationService.GetAllEntities();

            return locations.Select(location => new SelectListItem
            {
                Text = location.Name,
                Value = location.LocationId.ToString()
            }).ToList();
        }
    }
}