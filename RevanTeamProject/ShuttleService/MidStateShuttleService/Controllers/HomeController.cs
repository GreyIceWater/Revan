using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MidStateShuttleService.Models;
using MidStateShuttleService.Service;
using System.Diagnostics;

namespace MidStateShuttleService.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            // Fetch all feedback entries and order them by DateSubmitted in descending order
            //var feedbackList = _context.Feedbacks.OrderByDescending(f => f.DateSubmitted).ToList();
            //return View(feedbackList);
            // Fetch active feedback entries only
            var activeFeedbackList = _context.Feedbacks
                                      .Where(f => f.IsActive)
                                      .OrderByDescending(f => f.DateSubmitted)
                                      .ToList();
            RouteServices rs = new RouteServices(_context);
            ViewBag.RouteSchedule = rs.GetScheduleRoutes();

            return View(activeFeedbackList);
        }
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // POST: Feedback/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create([Bind("Comment,CustomerName,Rating")] Feedback feedback)
        {
            // DEV NOTE: ModelState ensures incoming form data passes validation rules defined on the Feedback model.
            if (ModelState.IsValid)
            {
                try
                {
                    // DEV NOTE: If the user leaves the name blank, store the testimonial as "Anonymous".
                    feedback.CustomerName = string.IsNullOrWhiteSpace(feedback.CustomerName)
                        ? "Anonymous"
                        : feedback.CustomerName;

                    // DEV NOTE: Public submissions should not appear on the site until approved by an admin.
                    feedback.IsActive = false;

                    // DEV NOTE: Display flag should be controlled by admin approval logic, not the public form.
                    feedback.DisplayTestimonial = false;

                    // DEV NOTE: Store timestamps in UTC so they can be converted for display later if needed.
                    feedback.DateSubmitted = DateTime.UtcNow;

                    // DEV NOTE: Add testimonial to database and persist the change.
                    _context.Add(feedback);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Testimonial successfully saved.");

                    // DEV NOTE: TempData flag used by the view to trigger the success modal.
                    TempData["FeedbackSuccess"] = "True";

                    // DEV NOTE: Increment feedback notification counter for the admin dashboard.
                    int feedbackCount = HttpContext.Session.GetInt32("FeedbackCount") ?? 0;
                    feedbackCount++;

                    HttpContext.Session.SetInt32("FeedbackCount", feedbackCount);
                    HttpContext.Session.SetString("LastFeedback", "You have a new feedback!");

                    // DEV NOTE: Redirect to Index to prevent form resubmission on page refresh.
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exception)
                {
                    // DEV NOTE: Log database or processing errors for troubleshooting.
                    _logger.LogError(exception, "Error saving testimonial.");
                }
            }
            else
            {
                // DEV NOTE: Log validation errors to help diagnose form submission issues.
                foreach (var modelStateKey in ViewData.ModelState.Keys)
                {
                    var modelStateValue = ViewData.ModelState[modelStateKey];

                    foreach (var error in modelStateValue.Errors)
                    {
                        _logger.LogError(error.ErrorMessage);
                    }
                }
            }

            // DEV NOTE: Reload approved testimonials so the Index page can render correctly after a failed submission.
            var activeFeedbackList = _context.Feedbacks
                .Where(feedbackItem => feedbackItem.IsActive)
                .OrderByDescending(feedbackItem => feedbackItem.DateSubmitted)
                .ToList();

            // DEV NOTE: Reload route schedule used by the home page.
            RouteServices routeService = new RouteServices(_context);
            ViewBag.RouteSchedule = routeService.GetScheduleRoutes();

            return View("Index", activeFeedbackList);
        }

        private string getSchedule()
        {
            RouteServices rs = new RouteServices(_context);
            try
            {

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Routes could not be retrieved");
                return "<h5>An error has occurred displaying route schedule at this time. Please try again later.";
            }



            return null;
        }

    }

}
