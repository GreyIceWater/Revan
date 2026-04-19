using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using MidStateShuttleService.Models;
using MidStateShuttleService.Service;
using MidStateShuttleService.Services;
using System.Net;
using System.Net.Mail;

namespace MidStateShuttleService.Controllers
{
    public class CommunicateController : Controller
    {
        private readonly EmailServices _emailServices;

        private readonly ILogger<CommunicateController> _logger;

        private readonly ApplicationDbContext _context;

        // Inject ApplicationDbContext into the controller constructor
        public CommunicateController(ApplicationDbContext context, ILogger<CommunicateController> logger, EmailServices emailServices)
        {
            _context = context; // Assign the injected ApplicationDbContext to the _context field
            _logger = logger; // Assign the injected ILogger to the _logger field
            _emailServices = emailServices;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Index(int? routeId)
        {
            _logger.LogInformation("CommunicateController Index GET accessed. RouteId: {RouteId}", routeId);

            var model = new CommuncateModel();
            model.LocationNames = GetLocationNames();

            if (routeId.HasValue)
            {
                var route = _context.Routes
                    .Include(r => r.PickUpLocation)
                    .Include(r => r.DropOffLocation)
                    .FirstOrDefault(r => r.RouteID == routeId.Value);

                if (route != null)
                {
                    _logger.LogInformation("Route found for communication. RouteId: {RouteId}", route.RouteID);

                    ViewData["RouteId"] = route.RouteID;
                    ViewData["RouteInfo"] = $"{route.PickUpLocation.Name} → {route.DropOffLocation.Name}";
                }
                else
                {
                    _logger.LogWarning("Route not found for communication. RouteId: {RouteId}", routeId);
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Index(CommuncateModel c, int? routeId)
        {
            _logger.LogInformation("CommunicateController Index POST received. RouteId: {RouteId}", routeId);

            c.LocationNames = GetLocationNames();
            if (ModelState.IsValid)
            {
                try
                {
                    CommunicationServices cs = new CommunicationServices(_context);
                    c.IsActive = true;
                    cs.AddEntity(c);

                    _logger.LogInformation("Communication message saved successfully. RouteId: {RouteId}", routeId);

                    RegisterServices rs = new RegisterServices(_context);
                    var registeredStudents = rs.GetEmailsByRoute(routeId ?? 0);

                    _logger.LogInformation("Found {StudentCount} registered students for RouteId: {RouteId}", registeredStudents.Count(), routeId);

                    foreach (var student in registeredStudents)
                    {
                        _logger.LogInformation("Sending communication email to {StudentEmail} for RouteId: {RouteId}", student.Email, routeId);
                        _emailServices.SendEmail(student.Email, "Mid State Shuttle Service Update", c.message);
                    }
                    TempData["CommunicationSuccess"] = true;

                    _logger.LogInformation("Communication processing completed successfully for RouteId: {RouteId}", routeId);

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error Sending Message");
                    return View("Error");
                }
            }

            _logger.LogWarning("CommunicateController Index POST failed validation. RouteId: {RouteId}", routeId);

            return View(c);
        }

        [AllowAnonymous]
        public IActionResult MessageSent()
        {
            _logger.LogInformation("MessageSent view accessed.");

            return View();
        }

        /// <summary>
        /// Displays the view for the student's communication form
        /// </summary>
        /// <returns> The Student Communicate View </returns>
        [AllowAnonymous]
        public IActionResult StudentCommunicate()
        {
            _logger.LogInformation("StudentCommunicate GET accessed.");

            return View();
        }

        // When the form submits, this method will play out.
        [AllowAnonymous]
        [HttpPost]
        public IActionResult StudentCommunicate(Message c)
        {
            _logger.LogInformation("StudentCommunicate POST received from Name: {Name}, Email: {Email}", c?.name, c?.Email);

            if (ModelState.IsValid)
            {
                try
                {
                    MessageServices ms = new MessageServices(_context);
                    c.IsActive = true;
                    ms.AddEntity(c);

                    _logger.LogInformation("Student message saved successfully. MessageId: {MessageId}", c.id);

                    // Increment the message count in the session
                    int messageCount = HttpContext.Session.GetInt32("MessageCount") ?? 0;
                    messageCount++;

                    HttpContext.Session.SetInt32("MessageCount", messageCount);
                    // Optionally, save the last message or a summary
                    HttpContext.Session.SetString("LastMessage", "You have a new message!");

                    HttpContext.Session.SetString("CommunicationSuccess", "true");

                    TempData["CommunicationSuccess"] = true;

                    Notification notif = new Notification();
                    notif.Subject = "Shuttle Service Review!";
                    notif.Body = c.name + " Sent a message!";
                    notif.TimeSent = DateTime.Now;
                    notif.MessageId = c.id;

                    new NotificationService(_context).SendNotification(notif);

                    _logger.LogInformation("Notification sent successfully for MessageId: {MessageId}", c.id);

                    return RedirectToAction("StudentCommunicate");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error Sending Message");

                    return View("Error");
                }
            }

            _logger.LogWarning("StudentCommunicate POST failed validation for Name: {Name}, Email: {Email}", c?.name, c?.Email);

            return View(c);
        }

        //The method which will get the location names from the database
        private IEnumerable<SelectListItem> GetLocationNames()
        {
            _logger.LogInformation("Fetching location names for communication dropdown.");

            LocationServices ls = new LocationServices(_context);
            var locations = ls.GetLocationNames();

            return locations;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult ViewAll(bool viewArchived = false)
        {
            _logger.LogInformation("ViewAll messages requested. ViewArchived: {ViewArchived}", viewArchived);

            var messages = _context.Messages.Where(m => m.IsActive == !viewArchived);

            ViewData["Archives"] = viewArchived;

            return View("MessagesTable", messages);
        }

        public IActionResult MessageRespond(int id)
        {
            _logger.LogInformation("MessageRespond GET requested for MessageId: {MessageId}", id);

            var message = _context.Messages.FirstOrDefault(m => m.id == id);
            if (message == null)
            {
                _logger.LogWarning("MessageRespond GET failed. Message not found for MessageId: {MessageId}", id);
                return NotFound();
            }
            return View(message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MessageRespond(int id, string responseMessage)
        {
            _logger.LogInformation("MessageRespond POST received for MessageId: {MessageId}", id);

            var message = _context.Messages.FirstOrDefault(m => m.id == id);
            if (message == null)
            {
                _logger.LogWarning("MessageRespond POST failed. Message not found for MessageId: {MessageId}", id);
                return NotFound();
            }

            try
            {
                string subject = "Message reply from Mid-State Shuttle Services";

                _emailServices.SendEmail(message.Email, subject, responseMessage);

                _logger.LogInformation("Response email sent successfully for MessageId: {MessageId}", id);

                TempData["Success"] = "Response sent successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send response for MessageId: {MessageId}", id);

                TempData["Error"] = $"Failed to send response: {ex.Message}";
                return View(message);
            }
        }

        // GET: DriverController/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            _logger.LogInformation("Delete requested for MessageId: {MessageId}", id);

            try
            {
                var message = _context.Messages.Find(id);

                if (message != null)
                {
                    message.IsActive = !message.IsActive; // Toggle IsActive from true to false or false to true
                    _context.SaveChanges();

                    _logger.LogInformation("Message IsActive toggled successfully for MessageId: {MessageId}. New IsActive: {IsActive}", id, message.IsActive);
                }
                else
                {
                    _logger.LogWarning("Delete failed. Message not found for MessageId: {MessageId}", id);

                    // Handle the case where the driver with the specified id is not found
                    ModelState.AddModelError("", "Message not found.");
                    return View();
                }

                return RedirectToAction("ViewAll");
            }
            catch (Exception ex)
            {
                // Log the exception
                LogEvents.LogSqlException(ex, (IWebHostEnvironment)_context);
                _logger.LogError(ex, "An error occurred while toggling IsActive of the message.");

                // Optionally add a model error for displaying an error message to the user
                ModelState.AddModelError("", "An unexpected error occurred while toggling IsActive of the driver, please try again.");

                // Return the view with an error message
                return View();
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Unarchive(int id)
        {
            _logger.LogInformation("Unarchive requested for MessageId: {MessageId}", id);

            var message = _context.Messages.Find(id);

            if (message == null)
            {
                _logger.LogWarning("Unarchive failed. Message not found for MessageId: {MessageId}", id);
                return NotFound();
            }

            message.IsActive = true;
            _context.SaveChanges();

            _logger.LogInformation("Message unarchived successfully for MessageId: {MessageId}", id);

            return RedirectToAction("ViewAll", new { viewArchived = true });
        }
    }
}