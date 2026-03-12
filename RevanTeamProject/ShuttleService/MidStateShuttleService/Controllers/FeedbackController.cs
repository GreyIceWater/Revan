using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MidStateShuttleService.Models;

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
            // Fetch all feedback entries and order them by DateSubmitted in descending order
            var feedbackList = _context.Feedbacks
                .Where(feedback => feedback.IsActive)
                .OrderByDescending(feedback => feedback.DateSubmitted)
                .ToList();
            return View(feedbackList);
        }

        // POST: Feedback/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create([Bind("Comment,CustomerName,Rating")] Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if CustomerName is null or empty, and set it to "Anonymous" if it is.
                    feedback.CustomerName = string.IsNullOrWhiteSpace(feedback.CustomerName) ? "Anonymous" : feedback.CustomerName;

                    feedback.DateSubmitted = DateTime.Now; // Set submission date to current date and time
                    feedback.IsActive = false;
                    _context.Add(feedback);
                    await _context.SaveChangesAsync();

                    // changing terminology to testimonial
                    _logger.LogInformation("Testimonial successfully saved.");

                    TempData["FeedbackSuccess"] = "True"; // Use TempData to signal that feedback was successful
                    return RedirectToAction(nameof(Index)); // Redirect back to the form page to show the success modal
                }
                catch (Exception ex)
                {
                    // changing terminology to testimonial
                    _logger.LogError(ex, "Error saving testimonial.");
                }
            }
            else
            {
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
            return View("Index", feedback);
        }


    }
}
