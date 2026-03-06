using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MidStateShuttleService.Models;
using MidStateShuttleService.Service;
using MidStateShuttleService.Services;
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
            AllModels allModels = new AllModels();

            LocationServices ls = new LocationServices(_context);
            allModels.Location = ls.GetAllEntities();

            RouteServices rs = new RouteServices(_context);
            allModels.Route = rs.GetAllEntities();

            DriverServices ds = new DriverServices(_context);
            allModels.Driver = ds.GetAllEntities();

            BusServices bs = new BusServices(_context);
            allModels.Bus = bs.GetAllEntities();

            CheckInServices cis = new CheckInServices(_context);
            allModels.CheckIn = cis.GetAllEntities();

            if (allModels.CheckIn != null)
            {
                foreach (var checkIn in allModels.CheckIn)
                {
                    checkIn.Date = TimeService.ConvertUtcToCentral(checkIn.Date);
                }
            }

            MessageServices ms = new MessageServices(_context);
            allModels.Message = ms.GetAllEntities();

            FeedbackServices fs = new FeedbackServices(_context);
            allModels.Feedback = fs.GetAllEntities();

            RegisterServices reg = new RegisterServices(_context);
            allModels.RegistrationViewModel = reg.GetViewModels();

            // Retrieve the registration success flag and count from the session
            var registrationSuccess = HttpContext.Session.GetString("RegistrationSuccess") == "true";
            int newRegistrations = HttpContext.Session.GetInt32("RegistrationCount") ?? 0;

            // You can now use registrationSuccess and registrationCountFromSession as needed
            // For instance, passing them to the view via ViewData or ViewBag, if your view logic depends on these values
            ViewData["RegistrationSuccess"] = registrationSuccess;
            ViewData["RegistrationCount"] = newRegistrations;
            ViewData["PastWeekRegistrations"] = reg.GetRegistrationCount("week");

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

            return View(allModels);

        }

        public ActionResult GetMessageDetails(int messageId)
        {
            // Fetch message details from the database based on the messageId
            var message = _context.Messages.Find(messageId);

            // Return a partial view with the message details
            return PartialView("_MessageDetails", message);
        }

        // Passenger lists are now being called reservation lists in the UI
        public ActionResult PassengerList(int id)
        {
            var route = _context.Routes
                .Include(r => r.PickUpLocation)
                .Include(r => r.DropOffLocation)
                .FirstOrDefault(r => r.RouteID == id);

            if (route == null)
            {
                return NotFound(); // Handle the case where the route is not found
            }

            // Initialize a HashSet to store unique register IDs
            var uniqueRegisterIds = new HashSet<int>();

            // Initialize a list to store unique passengers
            var uniquePassengers = new List<RegisterModel>();

            // Fetch passengers related to this route's selected route details
            var selectedRoutePassengers = _context.RegisterModels
                                    .Where(p => p.SelectedRouteDetail == route.RouteID.ToString())
                                    .ToList();

            // Fetch passengers related to this route's return route details
            var returnRoutePassengers = _context.RegisterModels
                                    .Where(p => p.ReturnSelectedRouteDetail == route.RouteID.ToString())
                                    .ToList();

            // Add passengers from selected route details
            foreach (var passenger in selectedRoutePassengers)
            {
                if (!uniqueRegisterIds.Contains(passenger.RegistrationId))
                {
                    uniquePassengers.Add(passenger);
                    uniqueRegisterIds.Add(passenger.RegistrationId);
                }
            }

            // Add passengers from return route details
            foreach (var passenger in returnRoutePassengers)
            {
                if (!uniqueRegisterIds.Contains(passenger.RegistrationId))
                {
                    uniquePassengers.Add(passenger);
                    uniqueRegisterIds.Add(passenger.RegistrationId);
                }
            }
            ViewBag.PassengerList = _context.RegisterModels.ToList();
            var pickupLocation = route.ToStringPickUp();
            var dropOffLocation = route.ToStringDropOff();

            var pickupLocationTime = route.ToStringPickUpTime();
            var dropOffLocationTime = route.ToStringDropOffTime();

            // Construct the title string
            // using Reservation list in the UI instead of passenger list
            ViewBag.Title = $"Reservation List for {pickupLocation} ({pickupLocationTime}) to {dropOffLocation} ({dropOffLocationTime})";

            // Pass the route and the list of unique passengers to the view
            ViewBag.Route = route;
            return View(uniquePassengers);
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

            // IMPORTANT: return the PARTIAL view here
            return PartialView("~/Views/Shared/DashboardPartials/ReportsTable.cshtml", allModels);
        }



        // ==========================================================
        // FULL REPORTS PAGE (Admin Only)
        // - Full page view with Layout + CSS
        // - ALL TIME data
        // ==========================================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Reports(string report = "")
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

            // IMPORTANT: this is the FULL page view
            return View("~/Views/Dashboard/Reports.cshtml", allModels);
        }



        // ==========================================================
        // EXPORT RIDER REQUESTS (CSV) - Admin Only, ALL TIME
        // ==========================================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ExportRiderRequestsCsv()
        {
            var rows = await _context.RegisterModels
                .AsNoTracking()
                .OrderByDescending(r => r.InsertDateTime)
                .Select(r => new
                {
                    r.RegistrationId,
                    r.Name,
                    r.StudentId,
                    r.Email,
                    r.Phone,
                    r.IsAdult,
                    r.TripType,
                    r.InsertDateTime,
                    r.IsArchived,
                    r.IsActive
                })
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("RegistrationId,Name,StudentId,Email,Phone,IsAdult,TripType,InsertDateTime,IsArchived,IsActive");

            foreach (var r in rows)
            {
                sb.AppendLine(
                    $"{r.RegistrationId}," +
                    $"{Csv(r.Name)}," +
                    $"{Csv(r.StudentId)}," +
                    $"{Csv(r.Email)}," +
                    $"{Csv(r.Phone)}," +
                    $"{r.IsAdult}," +
                    $"{Csv(r.TripType)}," +
                    $"{(r.InsertDateTime.HasValue ? r.InsertDateTime.Value.ToString("o") : "")}," +
                    $"{r.IsArchived}," +
                    $"{r.IsActive}"
                );
            }

            return File(
                System.Text.Encoding.UTF8.GetBytes(sb.ToString()),
                "text/csv",
                $"rider-requests-ALLTIME-{DateTime.Now:yyyyMMdd-HHmm}.csv"
            );
        }



        // ==========================================================
        // EXPORT CHECK-INS (CSV) - Admin Only, ALL TIME
        // ==========================================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ExportCheckInsCsv()
        {
            var rows = await _context.CheckIns
                .AsNoTracking()
                .OrderByDescending(c => c.Date)
                .Select(c => new
                {
                    c.CheckInId,
                    c.Name,
                    c.StudentId,
                    c.Date,
                    c.FirstTime,
                    c.LocationId,
                    c.DropOffLocationId,
                    c.Comments,
                    c.IsActive
                })
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("CheckInId,Name,StudentId,DateCentral,FirstTime,PickUpLocationId,DropOffLocationId,Comments,IsActive");

            foreach (var c in rows)
            {
                var centralTime = TimeService.ConvertUtcToCentral(c.Date);

                sb.AppendLine(
                    $"{c.CheckInId}," +
                    $"{Csv(c.Name)}," +
                    $"{Csv(c.StudentId)}," +
                    $"{centralTime:O}," +
                    $"{c.FirstTime}," +
                    $"{c.LocationId}," +
                    $"{c.DropOffLocationId}," +
                    $"{Csv(c.Comments)}," +
                    $"{c.IsActive}"
                );
            }

            return File(
                System.Text.Encoding.UTF8.GetBytes(sb.ToString()),
                "text/csv",
                $"checkins-ALLTIME-{DateTime.Now:yyyyMMdd-HHmm}.csv"
            );
        }



        // ==========================================================
        // CSV HELPER
        // ==========================================================
        private static string Csv(string? value)
        {
            value ??= "";

            var mustQuote =
                value.Contains(',') ||
                value.Contains('"') ||
                value.Contains('\n') ||
                value.Contains('\r');

            value = value.Replace("\"", "\"\"");

            return mustQuote ? $"\"{value}\"" : value;
        }

    }
}
