using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MidStateShuttleService.Models;
using MidStateShuttleService.Service;
using MidStateShuttleService.Services;

namespace MidStateShuttleService.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FeedbackController> _logger;

        // Constructor to inject the database context and logger
        public FeedbackController(ApplicationDbContext context, ILogger<FeedbackController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            _logger.LogInformation("Feedback Index accessed.");

            // Fetch all feedback entries and order them by DateSubmitted in descending order
            var feedbackList = _context.Feedbacks
                .Where(feedback => feedback.IsActive)
                .OrderByDescending(feedback => feedback.DateSubmitted)
                .ToList();

            _logger.LogInformation($"Feedback Index returning {feedbackList.Count} active testimonials.");

            return View(feedbackList);
        }

        // POST: Feedback/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create([Bind("Comment,CustomerName,Rating")] Feedback feedback)
        {
            _logger.LogInformation($"Feedback Create POST received. CustomerName: {feedback?.CustomerName}, Rating: {feedback?.Rating}");

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if CustomerName is null or empty, and set it to "Anonymous" if it is.
                    feedback.CustomerName = string.IsNullOrWhiteSpace(feedback.CustomerName) ? "Anonymous" : feedback.CustomerName;

                    feedback.DateSubmitted = DateTime.UtcNow; // Set submission date to current date and time
                    feedback.IsActive = false;
                    _context.Add(feedback);
                    await _context.SaveChangesAsync();

                    // changing terminology to testimonial
                    _logger.LogInformation("Testimonial successfully saved.");

                    Notification notif = new Notification();
                    notif.Subject = "Shuttle Service Review!";
                    notif.Body = feedback.CustomerName + " Just left a " + feedback.Rating + " star review.";
                    notif.TimeSent = DateTime.Now;
                    notif.FeedbackId = feedback.FeedbackId;

                    new NotificationService(_context).SendNotification(notif);

                    _logger.LogInformation($"Notification sent for FeedbackId: {feedback.FeedbackId}");

                    TempData["FeedbackSuccess"] = "True"; // Use TempData to signal that feedback was successful
                    return RedirectToAction("Index", "Home"); // Redirect back to the form page to show the success modal
                }
                catch (Exception ex)
                {
                    // changing terminology to testimonial
                    _logger.LogError(ex, "Error saving testimonial.");
                }
            }
            else
            {
                _logger.LogWarning("Feedback Create POST failed validation.");

                // Debugging code to log ModelState errors
                foreach (var modelStateKey in ViewData.ModelState.Keys)
                {
                    var modelStateVal = ViewData.ModelState[modelStateKey];
                    foreach (var error in modelStateVal.Errors)
                    {
                        _logger.LogError(error.ErrorMessage);
                    }
                }
            }
            // If we got this far, something failed, redisplay form
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult ViewAll()
        {
            _logger.LogInformation("Feedback ViewAll accessed.");

            var feedbacks = new FeedbackServices(_context).GetAllEntities().Where(f => f.IsActive);

            return View("FeedbackTable", feedbacks);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ArchiveFeedback(int id)
        {
            _logger.LogInformation($"ArchiveFeedback requested for FeedbackId: {id}");

            try
            {
                var feedback = _context.Feedbacks.Find(id);

                if (feedback != null)
                {
                    feedback.IsActive = !feedback.IsActive; // Toggle IsActive from true to false or false to true
                    _context.SaveChanges();

                    _logger.LogInformation($"ArchiveFeedback toggled successfully for FeedbackId: {id}. New IsActive: {feedback.IsActive}");
                }
                else
                {
                    _logger.LogWarning($"ArchiveFeedback failed. Feedback not found for FeedbackId: {id}");

                    // Handle the case where the driver with the specified id is not found
                    ModelState.AddModelError("", "Feedback not found.");
                    return View();
                }

                return RedirectToAction("ViewAll");
            }
            catch (Exception ex)
            {
                // Log the exception
                LogEvents.LogSqlException(ex, (IWebHostEnvironment)_context);
                _logger.LogError(ex, "An error occurred while toggling IsActive of the driver.");

                // Optionally add a model error for displaying an error message to the user
                ModelState.AddModelError("", "An unexpected error occurred while toggling IsActive of the feedback, please try again.");

                // Return the view with an error message
                return View();
            }
        }

        /// <summary>
        /// Approves the testimonial to display on the home page
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveTestimonial(int id)
        {
            _logger.LogInformation($"ApproveTestimonial requested for FeedbackId: {id}");

            try
            {
                var feedback = _context.Feedbacks.Find(id);

                if (feedback != null)
                {
                    feedback.DisplayTestimonial = true;
                    _context.SaveChanges();

                    _logger.LogInformation($"Testimonial approved for FeedbackId: {id}");
                }
                else
                {
                    _logger.LogWarning($"ApproveTestimonial failed. Feedback not found for FeedbackId: {id}");

                    // Handle the case where the feedback with the specified id is not found
                    ModelState.AddModelError("", "Feedback not found.");
                    return View();
                }

                return RedirectToAction("ViewAll");
            }
            catch (Exception ex)
            {
                // Log the exception
                LogEvents.LogSqlException(ex, (IWebHostEnvironment)_context);
                _logger.LogError(ex, "An error occurred while appriving a testimonial.");

                ModelState.AddModelError("", "An unexpected error occurred while aproving of the feedback, please try again.");

                // Return the view with an error message
                return View();
            }
        }
    }
}