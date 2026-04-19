using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MidStateShuttleService.Models;

namespace MidStateShuttleService.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;

        public NotificationController(
            ApplicationDbContext context,
            ILogger<NotificationController> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        // -------------------------
        // CREATE NOTIFICATION
        // -------------------------
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Notification notification)
        {
            _logger.LogInformation("Notification Create POST received.");

            if (notification == null)
            {
                _logger.LogWarning("Notification Create failed because notification was null.");
                return BadRequest("Notification is null");
            }

            try
            {
                notification.TimeSent = DateTime.Now;
                notification.IsArchived = false;

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Notification created successfully with Id: {NotificationId}", notification.Id);

                return Ok(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                return StatusCode(500, "Internal server error");
            }
        }

        // -------------------------
        // ARCHIVE NOTIFICATION
        // -------------------------
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Archive(int id)
        {
            _logger.LogInformation("Notification Archive requested for Id: {NotificationId}", id);

            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
            {
                _logger.LogWarning("Notification Archive failed. Notification not found for Id: {NotificationId}", id);
                return RedirectToAction("Index", "Dashboard");
            }

            try
            {
                notification.IsArchived = true;

                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Notification archived successfully for Id: {NotificationId}", id);

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving notification");
                return RedirectToAction("Index", "Dashboard");
            }
        }

        public IActionResult ViewNotificationContents(int id)
        {
            _logger.LogInformation("ViewNotificationContents requested for Id: {NotificationId}", id);

            var notification = _context.Notifications
                .FirstOrDefault(n => n.Id == id);

            if (notification == null)
            {
                _logger.LogWarning("ViewNotificationContents failed. Notification not found for Id: {NotificationId}", id);
                return RedirectToAction("Index", "Dashboard");
            }

            if (notification.FeedbackId.HasValue && notification.FeedbackId.Value != 0)
            {
                _logger.LogInformation("Notification {NotificationId} redirected to Feedback ViewAll.", id);
                return RedirectToAction("ViewAll", "Feedback");
            }

            if (notification.MessageId.HasValue && notification.MessageId.Value != 0)
            {
                _logger.LogInformation("Notification {NotificationId} redirected to Communicate ViewAll.", id);
                return RedirectToAction("ViewAll", "Communicate");
            }

            if (notification.RegistrationId.HasValue && notification.RegistrationId.Value != 0)
            {
                _logger.LogInformation("Notification {NotificationId} redirected to Register Details for RegistrationId: {RegistrationId}", id, notification.RegistrationId);

                return RedirectToAction(
                    "Details",
                    "Register",
                    new { registrationId = notification.RegistrationId }
                );
            }

            _logger.LogInformation("Notification {NotificationId} had no linked entity. Redirecting to Dashboard Index.", id);

            return RedirectToAction("Index", "Dashboard");
        }
    }
}