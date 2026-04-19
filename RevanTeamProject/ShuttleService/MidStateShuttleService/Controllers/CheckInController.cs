using Microsoft.AspNetCore.Authorization;
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
            _logger.LogInformation("Check-in page accessed.");

            ViewBag.Locations = GetLocationOptions(); // DEV NOTE: Dropdown population logic centralized below.
            return View();
        }

        [HttpPost]
        [AllowAnonymous] // DEV NOTE: Riders submit check-ins without authentication.
        [ValidateAntiForgeryToken]
        public IActionResult CheckIn(CheckIn submittedCheckIn)
        {
            _logger.LogInformation("Check-in submission received for Name: {Name}, StudentId: {StudentId}", submittedCheckIn?.Name, submittedCheckIn?.StudentId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Check-in submission failed validation for Name: {Name}, StudentId: {StudentId}", submittedCheckIn?.Name, submittedCheckIn?.StudentId);

                ViewBag.Locations = GetLocationOptions();

                TempData["Error"] = "Please fill out all required fields.";

                return View(submittedCheckIn);
            }

            submittedCheckIn.Date = DateTime.UtcNow;
            submittedCheckIn.IsActive = true;

            _checkInService.AddEntity(submittedCheckIn);

            _logger.LogInformation("Check-in created successfully for CheckInId: {CheckInId}, Name: {Name}", submittedCheckIn.CheckInId, submittedCheckIn.Name);

            int currentCheckInCount = HttpContext.Session.GetInt32("CheckInCount") ?? 0;
            HttpContext.Session.SetInt32("CheckInCount", currentCheckInCount + 1);

            TempData["Success"] = "Check-in successful!";

            return RedirectToAction(nameof(CheckIn));
        }

        [Authorize(Roles = "Admin")] // DEV NOTE: Only administrators can edit existing check-ins.
        [HttpGet]
        public IActionResult EditCheckIn(int id)
        {
            _logger.LogInformation("EditCheckIn GET requested for CheckInId: {CheckInId}", id);

            CheckIn existingCheckIn = _checkInService.GetEntityById(id);

            if (existingCheckIn == null)
            {
                _logger.LogWarning("EditCheckIn GET failed. Check-in not found for CheckInId: {CheckInId}", id);
                return FailedCheckIn("Check-in not found.");
            }

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
            _logger.LogInformation("EditCheckIn POST received for CheckInId: {CheckInId}", submittedModel.CheckInId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("EditCheckIn POST failed validation for CheckInId: {CheckInId}", submittedModel.CheckInId);
                submittedModel.LocationOptions = GetLocationOptions();
                return View(submittedModel);
            }

            CheckIn existingCheckIn = _checkInService.GetEntityById(submittedModel.CheckInId);

            if (existingCheckIn == null)
            {
                _logger.LogWarning("EditCheckIn POST failed. Check-in not found for CheckInId: {CheckInId}", submittedModel.CheckInId);
                return FailedCheckIn("Check-in not found.");
            }

            existingCheckIn.Name = submittedModel.Name;
            existingCheckIn.Comments = submittedModel.Comments;
            existingCheckIn.FirstTime = submittedModel.FirstTime;
            existingCheckIn.LocationId = submittedModel.LocationId;
            existingCheckIn.IsActive = true;

            // DEV NOTE: ViewModel stores UTC internally to keep DB timestamps consistent.
            existingCheckIn.Date = submittedModel.UtcDate;

            _checkInService.UpdateEntity(existingCheckIn);

            _logger.LogInformation("Check-in updated successfully for CheckInId: {CheckInId}", submittedModel.CheckInId);

            return RedirectToAction("ViewAll", "CheckIn");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Driver")]
        public ActionResult ViewAll(bool viewArchived = false)
        {
            _logger.LogInformation("ViewAll check-ins requested. viewArchived: {ViewArchived}", viewArchived);

            var checkins = _context.CheckIns
                .Include(c => c.Location)
                .Include(c => c.DropOffLocation)
                .Where(c => c.IsActive == !viewArchived)
                .ToList();

            ViewData["Archives"] = viewArchived;

            _logger.LogInformation("ViewAll returned {CheckInCount} check-ins. viewArchived: {ViewArchived}", checkins.Count, viewArchived);

            return View("CheckInTable", checkins);
        }

        [Authorize(Roles = "Admin")] // DEV NOTE: Admin-only operation that toggles check-in active state.
        [HttpPost] // DEV NOTE: Data modification endpoints should use POST instead of GET.
        [ValidateAntiForgeryToken]
        public IActionResult ToggleCheckInActive(int checkInId)
        {
            _logger.LogInformation("ToggleCheckInActive requested for CheckInId: {CheckInId}", checkInId);

            try
            {
                CheckIn existingCheckIn = _checkInService.GetEntityById(checkInId);

                if (existingCheckIn == null)
                {
                    _logger.LogWarning("ToggleCheckInActive failed. Check-in not found for CheckInId: {CheckInId}", checkInId);
                    return FailedCheckIn("Check-in could not be found.");
                }

                existingCheckIn.IsActive = !existingCheckIn.IsActive;

                _checkInService.UpdateEntity(existingCheckIn);

                _logger.LogInformation("ToggleCheckInActive succeeded for CheckInId: {CheckInId}. New IsActive: {IsActive}", checkInId, existingCheckIn.IsActive);

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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Unarchive(int id)
        {
            _logger.LogInformation("Unarchive requested for CheckInId: {CheckInId}", id);

            var checkin = _context.CheckIns.Find(id);

            if (checkin == null)
            {
                _logger.LogWarning("Unarchive failed. Check-in not found for CheckInId: {CheckInId}", id);
                return NotFound();
            }

            checkin.IsActive = true;
            _context.SaveChanges();

            _logger.LogInformation("Unarchive succeeded for CheckInId: {CheckInId}", id);

            return RedirectToAction("ViewAll", new { viewArchived = true });
        }

        [AllowAnonymous] // DEV NOTE: Shared error view for failed check-in operations.
        [HttpGet]
        public IActionResult FailedCheckIn(string errorMessage)
        {
            _logger.LogWarning("FailedCheckIn view returned with error message: {ErrorMessage}", errorMessage);

            ViewBag.ErrorMessage = errorMessage;
            return View("FailedCheckIn");
        }

        // DEV NOTE:
        // Helper method used to build location dropdown options.
        // If multiple controllers require this logic, it should be moved
        // into LocationServices as something like GetLocationSelectList().
        private List<SelectListItem> GetLocationOptions()
        {
            _logger.LogInformation("Loading active location options for check-in dropdown.");

            var locations = _context.Locations.Where(l => l.IsActive);

            return locations.Select(location => new SelectListItem
            {
                Text = location.Name,
                Value = location.LocationId.ToString()
            }).ToList();
        }
    }
}