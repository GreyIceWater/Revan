using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MidStateShuttleService.Models;
using MidStateShuttleService.Service;
using MidStateShuttleService.Services;
using MidStateShuttleService.ViewModels;
using System.Data;
using System.Diagnostics;

namespace MidStateShuttleService.Controllers
{
    [Authorize(Roles = "Admin,Driver")]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly EmailServices _emailServices;

        // Inject ApplicationDbContext into the controller constructor
        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger, EmailServices emailServices)
        {
            _context = context; // Assign the injected ApplicationDbContext to the _context field
            _logger = logger;
            _emailServices = emailServices;
        }

        public IConfiguration Configuration { get; }

        // GET: DashboardController
        public ActionResult Index(string section = "")
        {
            DashboardViewModel dashboardViewModel = new DashboardViewModel();

            CheckInServices cis = new CheckInServices(_context);

            RegisterServices reg = new RegisterServices(_context);

            MessageServices mes = new MessageServices(_context);

            NotificationService note = new NotificationService(_context);

            dashboardViewModel.TotalMonthlyCheckins = cis.GetAllEntities().Where(c => c.Date >= DateTime.Today.AddDays(-30)).Count();
            dashboardViewModel.PastWeekRegistrations = reg.GetRegistrationCount("week");
            dashboardViewModel.TotalRequests = reg.GetAllEntities().Where(r => !r.IsArchived).Count();

            if (User.IsInRole("Admin"))
            {
                dashboardViewModel.Messages = mes.GetAllEntities().ToList();
                dashboardViewModel.Notifications = note.GetAllEntities().Where(r => !r.IsArchived).ToList();
            }

            // Retrieve the registration success flag and count from the session
            var registrationSuccess = HttpContext.Session.GetString("RegistrationSuccess") == "true";
            int newRegistrations = HttpContext.Session.GetInt32("RegistrationCount") ?? 0;

            // You can now use registrationSuccess and registrationCountFromSession as needed
            // For instance, passing them to the view via ViewData or ViewBag, if your view logic depends on these values
            ViewData["RegistrationSuccess"] = registrationSuccess;
            ViewData["RegistrationCount"] = newRegistrations;

            // Retrieve the check-in count from the session
            int checkInCountFromSession = HttpContext.Session.GetInt32("CheckInCount") ?? 0;

            // Pass it to the view
            ViewData["CheckInCount"] = checkInCountFromSession;

            // Retrieve the message count and last message from the session
            int messageCountFromSession = HttpContext.Session.GetInt32("MessageCount") ?? 0;
            string lastMessage = HttpContext.Session.GetString("LastMessage") ?? "You have a new message!";

            // Pass them to the view
            ViewData["MessageCount"] = messageCountFromSession;
            ViewData["LastMessage"] = lastMessage;

            // Retrieve the feedback count and last feedback from the session
            int feedbackCountFromSession = HttpContext.Session.GetInt32("FeedbackCount") ?? 0;
            string lastFeedback = HttpContext.Session.GetString("LastFeedback") ?? "You have a new testimonial!";

            // Pass them to the view
            ViewData["FeedbackCount"] = feedbackCountFromSession;
            ViewData["LastFeedback"] = lastFeedback;

            // Log the value to ensure it's being received correctly
            _logger.LogInformation($"Section received: {section}");


            // Decide which section to open based on the 'section' parameter
            ViewBag.OpenSection = section;

            if (section == "feedback")
            {
                HttpContext.Session.SetInt32("FeedbackCount", 0); // Reset feedback count immediately when section is feedback
            }
            else if (section == "message")
            {
                HttpContext.Session.SetInt32("MessageCount", 0); // Reset message count
            }

            return View(dashboardViewModel);

        }

        public ActionResult ViewReports()
        {
            return View("Reports");
        }

        public ActionResult GetMessageDetails(int messageId)
        {
            // Fetch message details from the database based on the messageId
            var message = _context.Messages.Find(messageId);

            // Return a partial view with the message details
            return PartialView("_MessageDetails", message);
        }
        
        // Accept and reject feedback methods
        public async Task<IActionResult> AcceptFeedback(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                feedback.IsActive = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> RejectFeedback(int id)
        {
            try
            {
                var feedback = await _context.Feedbacks.FindAsync(id);
                if (feedback != null)
                {
                    feedback.IsActive = false;  // Set feedback as inactive
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                // Log the SQL exception and any other exceptions
                LogEvents.LogSqlException(ex, (IWebHostEnvironment)_context);
                _logger.LogError(ex, "An error occurred while deleting a testimonial.");

                // Optionally add a model error for displaying an error message to the user
                ModelState.AddModelError("", "An unexpected error occurred while deleting the testimonial, please try again.");

                // Return the view with an error message
                return View();
            }
        }

        // Add a function to explicitly reload the page when feedback is clicked
        public ActionResult FeedbackClicked()
        {
            ViewBag.OpenSection = "feedback";
            HttpContext.Session.SetInt32("FeedbackCount", 0);
            return RedirectToAction("Index", new { section = "feedback" }); // Redirect to Index to ensure changes take effect immediately
        }

        // ==========================================================
        // REPORTS PARTIAL LOADER (Admin Only)
        // - Called when admin clicks "Run" inside the dashboard widget
        // - Returns ONLY the partial (no layout, no <link> tags)
        // - ALL TIME data
        // ==========================================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ReportsPartial(string report = "")
        {
            var allModels = new AllModels();

            if (string.Equals(report, "requests", StringComparison.OrdinalIgnoreCase))
            {
                allModels.Register = await _context.RegisterModels
                    .AsNoTracking()
                    .OrderByDescending(r => r.InsertDateTime)
                    .ToListAsync();

                ViewBag.ReportType = "requests";
            }
            else if (string.Equals(report, "checkins", StringComparison.OrdinalIgnoreCase))
            {
                allModels.CheckIn = await _context.CheckIns
                    .AsNoTracking()
                    .Include(c => c.Location)
                    .Include(c => c.DropOffLocation)
                    .OrderByDescending(c => c.Date)
                    .ToListAsync();

                // Convert UTC -> Central for display consistency
                if (allModels.CheckIn != null)
                {
                    foreach (var checkIn in allModels.CheckIn)
                        checkIn.Date = TimeService.ConvertUtcToCentral(checkIn.Date);
                }

                ViewBag.ReportType = "checkins";
            }
            else
            {
                ViewBag.ReportType = "";
            }

            return View("ReportsTable.cshtml", allModels);
        }
    }
}
